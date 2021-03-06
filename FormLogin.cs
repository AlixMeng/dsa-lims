﻿/*	
	DSA Lims - Laboratory Information Management System
    Copyright (C) 2018  Norwegian Radiation Protection Authority

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
// Authors: Dag Robole,

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormLogin : Form
    {
        private DSASettings settings = null;
        private Guid mUserId = Guid.Empty;
        private string mUserName = String.Empty;
        private Guid mLabId = Guid.Empty;

        public Guid UserId { get { return mUserId; } }
        public string UserName { get { return mUserName; } }
        public Guid LabId { get { return mLabId; } }

        public FormLogin(DSASettings s)
        {
            InitializeComponent();
            settings = s;
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            cboxAction.SelectedIndex = 0;            
            ActiveControl = tbUsername;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            miExit_Click(sender, e);
        }        

        private void btnOk_Click(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = new SqlConnection(DB.ConnectionString);
                conn.Open();

                trans = conn.BeginTransaction();

                int client_version, database_version;

                GetVersionInfo(conn, trans, out client_version, out database_version);

                if (client_version != database_version)
                {
                    MessageBox.Show("Incompatible database. Client expects version " + client_version + " but database has version " + database_version);
                    return;
                }

                if (cboxAction.SelectedIndex == 0)
                {
                    string username = tbUsername.Text.ToLower().Trim();
                    string password = tbPassword.Text.Trim();

                    if (username.Length < Utils.MIN_USERNAME_LENGTH)
                    {
                        MessageBox.Show("Username must be at least 3 characters long");
                        return;
                    }

                    if (password.Length < Utils.MIN_PASSWORD_LENGTH)
                    {
                        MessageBox.Show("Authentication failed");
                        return;
                    }

                    if (!ValidateLimsUser(conn, trans, username, password))
                    {
                        MessageBox.Show("Authentication failed");
                        return;
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    if (!IsCurrentUserMachineAdmin() && !IsCurrentUserDomainAdmin()) // FIXME: Machine admin only for development
                    {
                        MessageBox.Show("Can not create the LIMSAdministrator user because you are not running as system administrator");
                        return;
                    }

                    if (CreateLIMSAdministrator(conn, trans))
                    {
                        cboxAction.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Creatint LIMS administrator failed");
                        return;
                    }
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }

        private void GetVersionInfo(SqlConnection conn, SqlTransaction trans, out int client_version, out int database_version)
        {
            client_version = Assembly.GetExecutingAssembly().GetName().Version.Major;

            SqlCommand cmd = new SqlCommand("select value from counters where name = 'database_version'", conn, trans);
            database_version = (int)cmd.ExecuteScalar();
        }

        private bool CreateLIMSAdministrator(SqlConnection conn, SqlTransaction trans)
        {
            FormCreateLIMSAdministrator form = new FormCreateLIMSAdministrator();
            if (form.ShowDialog() != DialogResult.OK)
                return false;            
                            
            Guid personId = Guid.Empty;
            Guid adminId = Guid.Empty;
            SqlCommand cmd = new SqlCommand("select id from person where name = 'LIMSAdministrator'", conn, trans);
            cmd.CommandType = System.Data.CommandType.Text;
            object o = cmd.ExecuteScalar();
            if (!DB.IsValidField(o))
            {
                personId = Guid.NewGuid();
                cmd.CommandText = "insert into person values(@id, @name, @email, @phone, @address, @create_date, @update_date)";
                cmd.Parameters.AddWithValue("@id", personId);
                cmd.Parameters.AddWithValue("@name", "LIMSAdministrator");
                cmd.Parameters.AddWithValue("@email", DBNull.Value);
                cmd.Parameters.AddWithValue("@phone", DBNull.Value);
                cmd.Parameters.AddWithValue("@address", DBNull.Value);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
            else
            {
                personId = Guid.Parse(o.ToString());
            }

            byte[] passwordHash = Utils.MakePasswordHash(form.SelectedPassword, "LIMSAdministrator");

            cmd = new SqlCommand("select id from account where person_id = @pid", conn, trans);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@pid", personId);
            o = cmd.ExecuteScalar();
            if (!DB.IsValidField(o))
            {
                adminId = Guid.NewGuid();
                cmd.CommandText = "insert into account values(@id, @username, @person_id, @laboratory_id, @language_code, @instance_status_id, @password_hash, @create_date, @update_date)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", adminId);
                cmd.Parameters.AddWithValue("@username", "LIMSAdministrator");
                cmd.Parameters.AddWithValue("@person_id", personId);
                cmd.Parameters.AddWithValue("@laboratory_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@language_code", "en");
                cmd.Parameters.AddWithValue("@instance_status_id", 1);
                cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
            }
            else
            {
                adminId = Guid.Parse(o.ToString());
                cmd.CommandText = "update account set password_hash = @password_hash, update_date = @update_date where id = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", adminId);
                cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
            }
            cmd.ExecuteNonQuery();

            return true;
        }

        private bool ValidateLimsUser(SqlConnection conn, SqlTransaction trans, string username, string password)
        {
            byte[] hash1 = Utils.MakePasswordHash(password, username);
            byte[] hash2 = null;
            Guid userId = Guid.Empty, labId = Guid.Empty;
            int accountStatus = 0;
            
            SqlCommand cmd = new SqlCommand("select id, laboratory_id, password_hash, instance_status_id from account where upper(username) = @username", conn, trans);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("@username", username.ToUpper());

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)                    
                    return false;

                reader.Read();

                userId = reader.GetGuid("id");
                labId = reader.GetGuid("laboratory_id");
                hash2 = reader.GetSqlBinary(2).Value;
                accountStatus = reader.GetInt32("instance_status_id");
            }

            if (accountStatus != InstanceStatus.Active)
                return false;

            if (!Utils.PasswordHashEqual(hash1, hash2))                
                return false;

            mUserId = userId;
            mUserName = username;
            mLabId = labId;                

            return true;
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }        

        private void tbUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                ActiveControl = tbPassword;
            }
        }

        private void tbPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                btnOk_Click(sender, e);
            }
        }

        public bool IsCurrentUserMachineAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {                
                // Require user to run app as admin
                WindowsPrincipal winPrincipal = new WindowsPrincipal(identity);
                if (winPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
                    return true;

                PrincipalContext ctx;
                try
                {
                    ctx = new PrincipalContext(ContextType.Machine);
                }
                catch
                {
                    return false;
                }

                var up = UserPrincipal.FindByIdentity(ctx, identity.Name);
                if (up != null)
                {
                    PrincipalSearchResult<Principal> authGroups = up.GetAuthorizationGroups();
                    return authGroups.Any(principal =>
                        principal.Sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) ||                        
                        principal.Sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid));
                }
            }

            return false;
        }

        public bool IsCurrentUserDomainAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {                
                try
                {
                    Domain.GetComputerDomain();                    
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    return false;
                }

                PrincipalContext ctx;
                try
                {
                    ctx = new PrincipalContext(ContextType.Domain);
                }
                catch
                {
                    return false;
                }

                var up = UserPrincipal.FindByIdentity(ctx, identity.Name);
                if (up != null)
                {
                    PrincipalSearchResult<Principal> authGroups = up.GetAuthorizationGroups();
                    return authGroups.Any(principal =>                         
                        principal.Sid.IsWellKnown(WellKnownSidType.AccountDomainAdminsSid) ||
                        principal.Sid.IsWellKnown(WellKnownSidType.AccountEnterpriseAdminsSid));
                }

                return false;
            }            
        }        

        private void cboxAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cboxAction.SelectedIndex == 1)
            {
                tbUsername.Text = "";
                tbUsername.Enabled = false;
                tbPassword.Text = "";
                tbPassword.Enabled = false;
            }
            else
            {                
                tbUsername.Enabled = true;
                tbPassword.Enabled = true;
            }
        }
    }
}
