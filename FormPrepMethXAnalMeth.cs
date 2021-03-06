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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormPrepMethXAnalMeth : Form
    {
        private Guid PreparationMethodId = Guid.Empty;
        private string PreparationMethodName = String.Empty;
        private List<Guid> ExistingAnalysisMethods = null;

        public FormPrepMethXAnalMeth(string preparationMethodName, Guid preparationMethodId, List<Guid> existingAnalysisMethods)
        {
            InitializeComponent();

            PreparationMethodId = preparationMethodId;
            PreparationMethodName = preparationMethodName;
            ExistingAnalysisMethods = existingAnalysisMethods;

            tbPreparationMethod.Text = PreparationMethodName;            
        }

        private void FormPrepMethXAnalMeth_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                var analMethArr = from item in ExistingAnalysisMethods select "'" + item + "'";
                string sanalmeth = string.Join(",", analMethArr);

                string query;
                if (String.IsNullOrEmpty(sanalmeth))
                    query = "select id, name from analysis_method order by name";
                else query = "select id, name from analysis_method where id not in(" + sanalmeth + ") order by name";

                using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text))
                {
                    lbAnalysisMethods.Items.Clear();

                    while (reader.Read())
                    {
                        var am = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                        lbAnalysisMethods.Items.Add(am);
                    }
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
            if (lbAnalysisMethods.SelectedItems.Count > 0)
            {
                SqlConnection conn = null;
                SqlTransaction trans = null;
                try
                {
                    conn = DB.OpenConnection();
                    trans = conn.BeginTransaction();

                    SqlCommand cmd = new SqlCommand("insert into preparation_method_x_analysis_method values(@preparation_method_id, @analysis_method_id)", conn, trans);

                    foreach (object item in lbAnalysisMethods.SelectedItems)
                    {
                        var selItem = item as Lemma<Guid, string>;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@preparation_method_id", PreparationMethodId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@analysis_method_id", selItem.Id, Guid.Empty);
                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans?.Rollback();
                    Common.Log.Error(ex);
                    MessageBox.Show(ex.Message);
                    return;
                }
                finally
                {
                    conn?.Close();
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
