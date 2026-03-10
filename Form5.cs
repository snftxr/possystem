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
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
            LoadRoles();
            LoadUsers();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            LoadRoles();
            LoadUsers();
        }

        private void LoadRoles()
        {
            const string sql = @"SELECT id, name FROM roles ORDER BY id";
            var dt = Db.Query(sql);
            comboBox1.DataSource = dt;
            comboBox1.DisplayMember = "name";
            comboBox1.ValueMember = "id";
        }

        private void LoadUsers()
        {
            const string sql = @"
SELECT u.id,
       u.username,
       u.password,
       r.name AS role_name
FROM users u
JOIN roles r ON r.id = u.role_id
ORDER BY u.id";

            var dt = Db.Query(sql);

            dataGridView1.Rows.Clear();
            foreach (DataRow row in dt.Rows)
            {
                dataGridView1.Rows.Add(
                    row["id"],
                    row["username"],
                    row["password"],
                    row["role_name"]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль.");
                return;
            }

            int roleId = (int)comboBox1.SelectedValue;

            const string sql = @"INSERT INTO users (username, password, role_id)
                                 VALUES (@username, @password, @role_id)";

            try
            {
                Db.Execute(sql,
                    new NpgsqlParameter("@username", username),
                    new NpgsqlParameter("@password", password),
                    new NpgsqlParameter("@role_id", roleId));

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении пользователя: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования.");
                return;
            }

            if (!int.TryParse(dataGridView1.CurrentRow.Cells[0].Value.ToString(), out int id))
            {
                MessageBox.Show("Некорректный идентификатор пользователя.");
                return;
            }

            string username = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль.");
                return;
            }

            int roleId = (int)comboBox1.SelectedValue;

            const string sql = @"UPDATE users
                                 SET username = @username,
                                     password = @password,
                                     role_id = @role_id
                                 WHERE id = @id";

            try
            {
                Db.Execute(sql,
                    new NpgsqlParameter("@id", id),
                    new NpgsqlParameter("@username", username),
                    new NpgsqlParameter("@password", password),
                    new NpgsqlParameter("@role_id", roleId));

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении пользователя: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.");
                return;
            }

            if (!int.TryParse(dataGridView1.CurrentRow.Cells[0].Value.ToString(), out int id))
            {
                MessageBox.Show("Некорректный идентификатор пользователя.");
                return;
            }

            if (MessageBox.Show("Удалить выбранного пользователя?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            const string sql = @"DELETE FROM users WHERE id = @id";

            try
            {
                Db.Execute(sql, new NpgsqlParameter("@id", id));
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении пользователя: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
