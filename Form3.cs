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
using Microsoft.VisualBasic;

namespace possystem
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            label3.Text = "итого: 0,00";
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            LoadProductsToCombo();
        }

        private void LoadProductsToCombo()
        {
            const string sql = @"SELECT id, name, price, stock FROM products WHERE stock > 0 ORDER BY name";
            var dt = Db.Query(sql);
            comboBox1.DataSource = dt;
            comboBox1.DisplayMember = "name";
            comboBox1.ValueMember = "id";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите товар.");
                return;
            }

            int productId = (int)comboBox1.SelectedValue;
            var row = ((DataRowView)comboBox1.SelectedItem).Row;
            string name = row["name"].ToString();
            decimal price = (decimal)row["price"];
            int stock = Convert.ToInt32(row["stock"]);

            string qtyStr = Interaction.InputBox("Введите количество:", "Количество", "1");
            if (!int.TryParse(qtyStr, out int qty) || qty <= 0)
            {
                MessageBox.Show("Некорректное количество.");
                return;
            }

            if (qty > stock)
            {
                MessageBox.Show("Недостаточное количество товара на складе.");
                return;
            }

            decimal sum = price * qty;

            int rowIndex = dataGridView1.Rows.Add(name, price, qty, sum);
            dataGridView1.Rows[rowIndex].Tag = productId;

            UpdateTotal();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Выберите строку для удаления.");
                return;
            }

            dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
            UpdateTotal();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            UpdateTotal();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Чек пуст.");
                return;
            }

            decimal total = 0;
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells[3].Value != null)
                {
                    total += Convert.ToDecimal(r.Cells[3].Value);
                }
            }

            using (var conn = Db.GetConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    var cmdSale = new NpgsqlCommand(
                        @"INSERT INTO sales (user_id, total_amount, sale_date)
                          VALUES (@user_id, @total, NOW())
                          RETURNING id", conn, tran);

                    cmdSale.Parameters.AddWithValue("@user_id", CurrentUser.Id);
                    cmdSale.Parameters.AddWithValue("@total", total);
                    int saleId = Convert.ToInt32(cmdSale.ExecuteScalar());

                    foreach (DataGridViewRow r in dataGridView1.Rows)
                    {
                        if (r.Cells[0].Value == null) continue;

                        int productId = (int)r.Tag;
                        int qty = Convert.ToInt32(r.Cells[2].Value);
                        decimal price = Convert.ToDecimal(r.Cells[1].Value);

                        var cmdItem = new NpgsqlCommand(
                            @"INSERT INTO sale_items (sale_id, product_id, quantity, price)
                              VALUES (@sale_id, @product_id, @qty, @price)", conn, tran);

                        cmdItem.Parameters.AddWithValue("@sale_id", saleId);
                        cmdItem.Parameters.AddWithValue("@product_id", productId);
                        cmdItem.Parameters.AddWithValue("@qty", qty);
                        cmdItem.Parameters.AddWithValue("@price", price);
                        cmdItem.ExecuteNonQuery();

                        var cmdStock = new NpgsqlCommand(
                            @"UPDATE products
                              SET stock = stock - @qty
                              WHERE id = @id", conn, tran);

                        cmdStock.Parameters.AddWithValue("@id", productId);
                        cmdStock.Parameters.AddWithValue("@qty", qty);
                        cmdStock.ExecuteNonQuery();
                    }

                    tran.Commit();
                    MessageBox.Show("Продажа успешно оформлена.");

                    dataGridView1.Rows.Clear();
                    UpdateTotal();
                    LoadProductsToCombo();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Ошибка при сохранении продажи: " + ex.Message);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells[3].Value != null)
                    total += Convert.ToDecimal(r.Cells[3].Value);
            }

            label3.Text = $"итого: {total:0.00}";
        }
    }
}
