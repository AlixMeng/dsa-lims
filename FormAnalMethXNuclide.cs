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
    public partial class FormAnalMethXNuclide : Form
    {
        private Guid AnalysisMethodId = Guid.Empty;
        private string AnalysisMethodName = String.Empty;
        private List<Guid> ExistingNuclides = null;

        public FormAnalMethXNuclide(string analysisMethodName, Guid analysisMethodId, List<Guid> existingNuclides)
        {
            InitializeComponent();

            AnalysisMethodId = analysisMethodId;
            AnalysisMethodName = analysisMethodName;
            ExistingNuclides = existingNuclides;

            tbAnalysisMethod.Text = AnalysisMethodName;            
        }

        private void FormAnalMethXNuclide_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                var nuclArr = from item in ExistingNuclides select "'" + item + "'";
                string snucl = string.Join(",", nuclArr);

                string query;
                if (String.IsNullOrEmpty(snucl))
                    query = "select id, name from nuclide where instance_status_id < 2 order by name";
                else query = "select id, name from nuclide where instance_status_id < 2 and id not in(" + snucl + ") order by name";

                using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text))
                {
                    lbNuclides.Items.Clear();

                    while (reader.Read())
                    {
                        var pm = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                        lbNuclides.Items.Add(pm);
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
            if (lbNuclides.SelectedItems.Count > 0)
            {
                SqlConnection conn = null;
                try
                {
                    conn = DB.OpenConnection();
                    SqlCommand cmd = new SqlCommand("insert into analysis_method_x_nuclide values(@analysis_method_id, @nuclide_id)", conn);

                    foreach (object item in lbNuclides.SelectedItems)
                    {
                        var selItem = item as Lemma<Guid, string>;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@analysis_method_id", AnalysisMethodId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@nuclide_id", selItem.Id, Guid.Empty);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
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
