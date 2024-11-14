using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace Registration_UI
{
    public partial class RegistrationUI : Form
    {
        public RegistrationUI()
        {
            InitializeComponent();
            LoadUserList(); // Load users when the form is initialized

        }
        OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=db_users.mdb");
        OleDbCommand cmd = new OleDbCommand();
        OleDbDataAdapter da = new OleDbDataAdapter();

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {
            new frmLogin().Show();
            this.Hide();
        }
        private void CheckbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckbxShowPas.Checked)
            {
                txtPassword.PasswordChar = '\0'; // Show password
                txtComPassword.PasswordChar = '\0'; // Show confirm password
            }
            else
            {
                txtPassword.PasswordChar = '•'; // Hide password
                txtComPassword.PasswordChar = '•'; // Hide confirm password
            }
        }
        private bool IsValidPassword(string password)
        {
            // Check if password is at least 8 characters, contains at least one uppercase letter and one number
            return password.Length >= 8
                   && password.Any(char.IsUpper)
                   && password.Any(char.IsDigit);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == "" || txtPassword.Text == "" || txtComPassword.Text == "")
            {
                MessageBox.Show("Username or Password fields are empty", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Exit early
            }
            else if (!IsValidPassword(txtPassword.Text))
            {
                MessageBox.Show("Password must be at least 8 characters long, contain at least one uppercase letter, and at least one number.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Focus();
                return; // Exit early
            }
            else if (txtPassword.Text == txtComPassword.Text)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                con.Open();

                // Check if username already exists
                string checkUser = "SELECT COUNT(*) FROM tbl_users WHERE username = @username";
                OleDbCommand checkCmd = new OleDbCommand(checkUser, con);
                checkCmd.Parameters.AddWithValue("@username", txtUsername.Text);
                int userExists = (int)checkCmd.ExecuteScalar();

                if (userExists > 0)
                {
                    MessageBox.Show("Username already exists, please choose another one", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtUsername.Focus();
                    con.Close();
                    return; // Exit early
                }
                else
                {
                    // Insert new user with parameters
                    string register = "INSERT INTO tbl_users ([username], [password]) VALUES (@username, @password);";
                    cmd = new OleDbCommand(register, con);
                    cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                    cmd.ExecuteNonQuery();

                    // Close connection, clear fields, show success message, and navigate to frmLogin
                    con.Close();
                    txtUsername.Text = "";
                    txtPassword.Text = "";
                    txtComPassword.Text = "";
                    MessageBox.Show("Your Account has been Successfully Created", "Registration Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    
                    LoadUserList(); 

                    // Navigate to frmLogin
                    frmLogin loginForm = new frmLogin();
                    loginForm.Show();
                    this.Hide(); 
                }

                // Ensure connection is closed
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            else
            {
                MessageBox.Show("Passwords do not match, Please Re-enter", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Text = "";
                txtComPassword.Text = "";
                txtPassword.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtComPassword.Text = "";
            txtUsername.Focus();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
        private void LoadUserList()
        {
            try
            {
                con.Open();
                string query = "SELECT username FROM tbl_users";
                OleDbCommand cmd = new OleDbCommand(query, con);
                OleDbDataReader dr = cmd.ExecuteReader();

                lstUsers.Items.Clear(); // Clear existing items in case of reload
                while (dr.Read())
                {
                    lstUsers.Items.Add(dr["username"].ToString()); // Add each username to the ListBox
                }

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void frmLogin_Load(object sender, EventArgs e)
        {
            LoadUserList();
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string selectedUser = lstUsers.SelectedItem.ToString();

                // Confirm deletion
                DialogResult result = MessageBox.Show($"Are you sure you want to delete the user '{selectedUser}'?",
                                                      "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        con.Open();
                        string deleteQuery = "DELETE FROM tbl_users WHERE username = @username";
                        OleDbCommand cmd = new OleDbCommand(deleteQuery, con);
                        cmd.Parameters.AddWithValue("@username", selectedUser);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show($"User '{selectedUser}' has been deleted successfully.", "Deletion Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Remove from ListBox
                        lstUsers.Items.Remove(selectedUser);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting user: " + ex.Message);
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open)
                        {
                            con.Close();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to delete.", "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
