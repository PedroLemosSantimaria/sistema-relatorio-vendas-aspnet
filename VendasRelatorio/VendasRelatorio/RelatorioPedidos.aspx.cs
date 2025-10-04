using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Reporting.WebForms;

public partial class RelatorioPedidos : System.Web.UI.Page
{
    private string connStr = ConfigurationManager.ConnectionStrings["VendasConn"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindClientes();
            LoadReport(null, null, null);
        }
    }

    private void BindClientes()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = "SELECT ClienteID, Nome FROM Clientes ORDER BY Nome";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlCliente.DataSource = dt;
                ddlCliente.DataTextField = "Nome";
                ddlCliente.DataValueField = "ClienteID";
                ddlCliente.DataBind();
                ddlCliente.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Todos --", ""));
            }
        }
    }

    private DataTable GetReportData(int? clienteId, DateTime? dataInicio, DateTime? dataFim)
    {
        DataTable dt = new DataTable();
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = @"
                SELECT c.Nome AS Cliente, p.DataPedido, pr.Nome AS Produto,
                       i.Quantidade, (i.Quantidade * i.PrecoUnitario) AS TotalItem
                FROM Clientes c
                JOIN Pedidos p ON c.ClienteID = p.ClienteID
                JOIN ItensPedido i ON p.PedidoID = i.PedidoID
                JOIN Produtos pr ON i.ProdutoID = pr.ProdutoID
                WHERE (@ClienteId IS NULL OR c.ClienteID = @ClienteId)
                  AND (@DataInicio IS NULL OR p.DataPedido >= @DataInicio)
                  AND (@DataFim IS NULL OR p.DataPedido <= @DataFim)
                ORDER BY c.Nome, p.DataPedido;";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ClienteId", (object)clienteId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataInicio", (object)dataInicio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataFim", (object)dataFim ?? DBNull.Value);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
        }
        return dt;
    }

    private void LoadReport(int? clienteId, DateTime? dataInicio, DateTime? dataFim)
    {
        DataTable dt = GetReportData(clienteId, dataInicio, dataFim);

        ReportViewer1.Reset();
        ReportViewer1.ProcessingMode = ProcessingMode.Local;
        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/RelatorioPedidos.rdlc");

        ReportDataSource rds = new ReportDataSource("DataSetPedidos", dt); // o nome "DataSetPedidos" deve bater com o RDLC
        ReportViewer1.LocalReport.DataSources.Clear();
        ReportViewer1.LocalReport.DataSources.Add(rds);

        ReportViewer1.LocalReport.Refresh();
    }

    protected void btnFiltrar_Click(object sender, EventArgs e)
    {
        int? clienteId = string.IsNullOrEmpty(ddlCliente.SelectedValue) ? (int?)null : int.Parse(ddlCliente.SelectedValue);
        DateTime? dataInicio = null, dataFim = null;
        DateTime temp;
        if (DateTime.TryParse(txtDataInicio.Text, out temp)) dataInicio = temp;
        if (DateTime.TryParse(txtDataFim.Text, out temp)) dataFim = temp;

        LoadReport(clienteId, dataInicio, dataFim);
    }

    protected void btnExportPdf_Click(object sender, EventArgs e)
    {
        // Gera o relatório (com os mesmos filtros atuais) e faz download em PDF
        int? clienteId = string.IsNullOrEmpty(ddlCliente.SelectedValue) ? (int?)null : int.Parse(ddlCliente.SelectedValue);
        DateTime? dataInicio = null, dataFim = null;
        DateTime temp;
        if (DateTime.TryParse(txtDataInicio.Text, out temp)) dataInicio = temp;
        if (DateTime.TryParse(txtDataFim.Text, out temp)) dataFim = temp;

        DataTable dt = GetReportData(clienteId, dataInicio, dataFim);

        LocalReport lr = new LocalReport();
        lr.ReportPath = Server.MapPath("~/Reports/RelatorioPedidos.rdlc");
        lr.DataSources.Clear();
        lr.DataSources.Add(new ReportDataSource("DataSetPedidos", dt));

        string mimeType, encoding, fileNameExtension;
        string[] streams;
        Warning[] warnings;

        byte[] renderedBytes = lr.Render(
            "PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

        Response.Clear();
        Response.ContentType = "application/pdf";
        Response.AddHeader("content-disposition", "attachment; filename=RelatorioPedidos.pdf");
        Response.BinaryWrite(renderedBytes);
        Response.End();
    }
}
