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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormUser : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid UserId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string UserName
        {
            get { return p.ContainsKey("username") ? p["username"].ToString() : String.Empty; }
        }

        public FormUser()
        {
            InitializeComponent();

            Text = "DSA-Lims - Create user";

            tbUsername.ReadOnly = false;
            tbPassword1.Enabled = true;
            tbPassword2.Enabled = true;            
        }

        public FormUser(Guid uid)
        {
            InitializeComponent();

            Text = "DSA-Lims - Edit user";

            p["id"] = uid;

            tbUsername.ReadOnly = true;
            tbPassword1.Enabled = false;
            tbPassword2.Enabled = false;            
            cboxPersons.Enabled = false;
        }

        private void FormUser_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                if (p.ContainsKey("id"))
                {                    
                    cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);

                    UI.PopulateComboBoxes(conn, "csp_select_persons_short", new SqlParameter[] { }, cboxPersons);

                    UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxLaboratory);

                    SqlCommand cmd = new SqlCommand("csp_select_account", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", p["id"]);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            throw new Exception("Account with id " + p["id"] + " was not found");

                        reader.Read();

                        tbUsername.Text = reader.GetString("username");
                        cboxPersons.SelectedValue = reader.GetGuid("person_id");
                        cboxLaboratory.SelectedValue = reader.GetGuid("laboratory_id");
                        cboxLanguage.Text = reader.GetString("language_code");
                        cboxInstanceStatus.SelectedValue = reader.GetInt32("instance_status_id");
                        p["create_date"] = reader["create_date"];
                        p["update_date"] = reader["update_date"];
                    }                
                }
                else
                {                    
                    cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);

                    UI.PopulateComboBoxes(conn, "csp_select_persons_short", new SqlParameter[] { }, cboxPersons);

                    UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Inactive)
                    }, cboxLaboratory);                

                    cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Abort;
                Close();
            }
            finally
            {
                conn?.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbUsername.Text.Trim()))
            {
                MessageBox.Show("Username is mandatory");
                return;
            }

            if (tbUsername.Text.Length < 3)
            {
                MessageBox.Show("Username must be at least 3 characters long");
                return;
            }

            if (!Utils.IsValidGuid(cboxPersons.SelectedValue))
            {
                MessageBox.Show("Person is mandatory");
                return;
            }

            p["person_id"] = cboxPersons.SelectedValue;
            p["username"] = tbUsername.Text;
            p["laboratory_id"] = cboxLaboratory.SelectedValue;
            p["language_code"] = cboxLanguage.Text.Trim();
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;

            bool success;
            if (!p.ContainsKey("id"))
            {
                if (tbPassword1.Text.Length < Utils.MIN_PASSWORD_LENGTH)
                {
                    MessageBox.Show("Password must be at least 8 characters long");
                    return;
                }

                if (tbPassword1.Text.CompareTo(tbPassword2.Text) != 0)
                {
                    MessageBox.Show("Passwords are not equal");
                    return;
                }

                p["password_hash"] = Utils.MakePasswordHash(tbPassword1.Text.Trim(), tbUsername.Text.Trim());

                success = InsertAccount();
            }
            else
            {
                success = UpdateAccount();
            }

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertAccount()
        {
            SqlConnection connection = null;

            try
            {
                p["create_date"] = DateTime.Now;                
                p["update_date"] = DateTime.Now;                

                connection = DB.OpenConnection();

                SqlCommand cmd = new SqlCommand("select count(*) from account where username = @username", connection);
                cmd.Parameters.AddWithValue("@username", p["username"]);
                int n = (int)cmd.ExecuteScalar();
                if(n > 0)
                {
                    MessageBox.Show("Username " + p["username"] + " already exists");
                    return false;
                }

                cmd.CommandText = "csp_insert_account";
                cmd.CommandType = CommandType.StoredProcedure;
                p["id"] = Guid.NewGuid();
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@username", p["username"]);
                cmd.Parameters.AddWithValue("@person_id", p["person_id"], Guid.Empty);
                cmd.Parameters.AddWithValue("@laboratory_id", p["laboratory_id"], Guid.Empty);
                cmd.Parameters.AddWithValue("@language_code", p["language_code"], String.Empty);
                cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
                cmd.Parameters.AddWithValue("@password_hash", p["password_hash"]);
                cmd.Parameters.AddWithValue("@create_date", p["create_date"]);                
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {                
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private bool UpdateAccount()
        {
            SqlConnection connection = null;            

            try
            {
                p["update_date"] = DateTime.Now;                

                connection = DB.OpenConnection();

                SqlCommand cmd = new SqlCommand("csp_update_account", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@laboratory_id", p["laboratory_id"], Guid.Empty);
                cmd.Parameters.AddWithValue("@language_code", p["language_code"], String.Empty);
                cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {                
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }        
    }
}
