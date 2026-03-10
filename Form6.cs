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
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            label4.Text = "сумма выручки: 0,00";
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime dateFrom = dateTimePicker1.Value.Date;
            DateTime dateTo = dateTimePicker2.Value.Date.AddDays(1).AddTicks(-1);

            const string sql = @"
SELECT s.id,
       u.username,
       s.total_amount,
       s.sale_date
FROM sales s
JOIN users u ON u.id = s.user_id
WHERE s.sale_date BETWEEN @from AND @to
ORDER BY s.sale_date";

            try
            {
                var dt = Db.Query(sql,
                    new NpgsqlParameter("@from", dateFrom),
                    new NpgsqlParameter("@to", dateTo));

                dataGridView1.Rows.Clear();

                decimal sum = 0;
                foreach (DataRow row in dt.Rows)
                {
                    dataGridView1.Rows.Add(
                        row["id"],
                        row["username"],
                        row["total_amount"],
                        row["sale_date"]);

                    sum += Convert.ToDecimal(row["total_amount"]);
                }

                label4.Text = $"сумма выручки: {sum:0.00}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке отчёта: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
