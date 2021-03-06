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
    public partial class FormUserXAnalMeth : Form
    {
        Guid mAccountId = Guid.Empty;

        public FormUserXAnalMeth(Guid accountId)
        {
            InitializeComponent();

            mAccountId = accountId;
        }

        private void FormUserXAnalMeth_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateAnalysisMethods(conn, gridAnalMeth);
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
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("delete from account_x_analysis_method where account_id = @acc_id", conn, trans);
                cmd.Parameters.AddWithValue("@acc_id", mAccountId);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into account_x_analysis_method values(@account_id, @analysis_method_id)";

                foreach (DataGridViewRow row in gridAnalMeth.SelectedRows)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@account_id", mAccountId);
                    cmd.Parameters.AddWithValue("@analysis_method_id", Utils.MakeGuid(row.Cells["id"].Value));
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

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
