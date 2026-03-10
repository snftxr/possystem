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
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void LoadProducts()
        {
            const string sql = @"SELECT id, name, description, price, stock FROM products ORDER BY id";
            var dt = Db.Query(sql);

            dataGridView1.Rows.Clear();
            foreach (DataRow row in dt.Rows)
            {
                dataGridView1.Rows.Add(
                    row["id"],
                    row["name"],
                    row["description"],
                    row["price"],
                    row["stock"]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            string description = textBox2.Text.Trim();

            if (!decimal.TryParse(textBox3.Text, out decimal price))
            {
                MessageBox.Show("Некорректная цена.");
                return;
            }

            if (!int.TryParse(textBox4.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Некорректное количество на складе.");
                return;
            }

            const string sql = @"INSERT INTO products (name, description, price, stock)
                                 VALUES (@name, @description, @price, @stock)";

            try
            {
                Db.Execute(sql,
                    new NpgsqlParameter("@name", name),
                    new NpgsqlParameter("@description", (object)description ?? DBNull.Value),
                    new NpgsqlParameter("@price", price),
                    new NpgsqlParameter("@stock", stock));

                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении товара: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Выберите товар для редактирования.");
                return;
            }

            if (!int.TryParse(dataGridView1.CurrentRow.Cells[0].Value.ToString(), out int id))
            {
                MessageBox.Show("Некорректный идентификатор товара.");
                return;
            }

            string name = textBox1.Text.Trim();
            string description = textBox2.Text.Trim();

            if (!decimal.TryParse(textBox3.Text, out decimal price))
            {
                MessageBox.Show("Некорректная цена.");
                return;
            }

            if (!int.TryParse(textBox4.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Некорректное количество на складе.");
                return;
            }

            const string sql = @"UPDATE products
                                 SET name = @name,
                                     description = @description,
                                     price = @price,
                                     stock = @stock
                                 WHERE id = @id";

            try
            {
                Db.Execute(sql,
                    new NpgsqlParameter("@id", id),
                    new NpgsqlParameter("@name", name),
                    new NpgsqlParameter("@description", (object)description ?? DBNull.Value),
                    new NpgsqlParameter("@price", price),
                    new NpgsqlParameter("@stock", stock));

                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении товара: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Выберите товар для удаления.");
                return;
            }

            if (!int.TryParse(dataGridView1.CurrentRow.Cells[0].Value.ToString(), out int id))
            {
                MessageBox.Show("Некорректный идентификатор товара.");
                return;
            }

            if (MessageBox.Show("Удалить выбранный товар?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            const string sql = @"DELETE FROM products WHERE id = @id";

            try
            {
                Db.Execute(sql, new NpgsqlParameter("@id", id));
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении товара: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
