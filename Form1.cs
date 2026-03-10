using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace possystem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var username = textBox1.Text.Trim();
            var password = textBox2.Text.Trim(); 

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                const string sql = @"
SELECT u.id,
       u.username,
       r.name AS role_name
FROM users u
JOIN roles r ON r.id = u.role_id
WHERE u.username = @username AND u.password = @password";

                var dt = Db.Query(sql,
                    new NpgsqlParameter("@username", username),
                    new NpgsqlParameter("@password", password));

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка входа",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var row = dt.Rows[0];
                CurrentUser.Id = Convert.ToInt32(row["id"]);
                CurrentUser.Username = row["username"].ToString();
                CurrentUser.RoleName = row["role_name"].ToString();

                var mainForm = new Form2();
                mainForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
