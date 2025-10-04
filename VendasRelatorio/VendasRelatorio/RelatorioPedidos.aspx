<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RelatorioPedidos.aspx.cs" Inherits="RelatorioPedidos" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Relatório de Pedidos</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div style="margin:10px;">
            <asp:DropDownList ID="ddlCliente" runat="server"></asp:DropDownList>
            <asp:TextBox ID="txtDataInicio" runat="server" placeholder="yyyy-mm-dd"></asp:TextBox>
            <asp:TextBox ID="txtDataFim" runat="server" placeholder="yyyy-mm-dd"></asp:TextBox>
            <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" OnClick="btnFiltrar_Click" />
            <asp:Button ID="btnExportPdf" runat="server" Text="Exportar PDF" OnClick="btnExportPdf_Click" />
        </div>

        <div style="margin:10px;">
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%" Height="700px" ProcessingMode="Local" />
        </div>
    </form>
</body>
</html>
