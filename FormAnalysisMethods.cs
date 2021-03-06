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
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormAnalysisMethods : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid AnalysisMethodId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string AnalysisMethodName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormAnalysisMethods()
        {
            InitializeComponent();
            Text = "Create analysis method";            
        }

        public FormAnalysisMethods(Guid amid)
        {
            InitializeComponent();            
            Text = "Update analysis method";
            p["id"] = amid;
        }

        private void FormAnalysisMethods_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                if (p.ContainsKey("id"))
                {                    
                    cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);

                    SqlCommand cmd = new SqlCommand("csp_select_analysis_method", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", p["id"]);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            throw new Exception("Analysis method with ID " + p["id"] + " was not found");

                        reader.Read();
                        tbName.Text = reader.GetString("name");
                        tbShortName.Text = reader.GetString("name_short");
                        tbDescriptionLink.Text = reader.GetString("description_link");
                        tbSpecRefRegExp.Text = reader.GetString("specter_reference_regexp");
                        cboxInstanceStatus.SelectedValue = reader.GetInt32("instance_status_id");
                        tbComment.Text = reader.GetString("comment");
                        p["create_date"] = reader.GetDateTime("create_date");
                        p["create_id"] = reader.GetGuid("create_id");
                        p["update_date"] = reader.GetDateTime("update_date");
                        p["update_id"] = reader.GetGuid("update_id");
                    }
                }
                else
                {
                    cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);                    
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
            if (String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Name is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbShortName.Text.Trim()))
            {
                MessageBox.Show("Short name is mandatory");
                return;
            }

            p["name"] = tbName.Text.Trim();
            p["name_short"] = tbShortName.Text.Trim();
            p["description_link"] = tbDescriptionLink.Text.Trim();
            p["specter_reference_regexp"] = tbSpecRefRegExp.Text.Trim();
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;
            p["comment"] = tbComment.Text.Trim();

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                if (DB.NameExists(connection, transaction, "analysis_method", p["name"].ToString(), AnalysisMethodId))
                {
                    MessageBox.Show("The analysis method '" + p["name"] + "' already exists");
                    return;
                }                
                
                if (!p.ContainsKey("id"))
                    InsertAnalysisMethod(connection, transaction);
                else
                    UpdateAnalysisMethod(connection, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                success = false;
                transaction?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private void InsertAnalysisMethod(SqlConnection conn, SqlTransaction trans)
        {            
            p["create_date"] = DateTime.Now;
            p["create_id"] = Common.UserId;
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;

            SqlCommand cmd = new SqlCommand("csp_insert_analysis_method", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            p["id"] = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@name_short", p["name_short"]);
            cmd.Parameters.AddWithValue("@description_link", p["description_link"], String.Empty);
            cmd.Parameters.AddWithValue("@specter_reference_regexp", p["specter_reference_regexp"], String.Empty);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);
            cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
            cmd.Parameters.AddWithValue("@create_id", p["create_id"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();                        
        }

        private void UpdateAnalysisMethod(SqlConnection conn, SqlTransaction trans)
        {
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;

            SqlCommand cmd = new SqlCommand("csp_update_analysis_method", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@name_short", p["name_short"]);
            cmd.Parameters.AddWithValue("@description_link", p["description_link"], String.Empty);
            cmd.Parameters.AddWithValue("@specter_reference_regexp", p["specter_reference_regexp"], String.Empty);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();                    
        }        
    }
}
