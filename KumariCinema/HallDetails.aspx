<%@ Page Title="Hall Management" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="HallDetails.aspx.cs" Inherits="KumariCinema.HallDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-container { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 32px; margin-bottom: 32px; max-width: 600px; }
        .form-group { margin-bottom: 20px; }
        .form-group label { display: block; margin-bottom: 8px; color: #ffffff; font-weight: 500; }
        .form-group input, .form-group select { width: 100%; padding: 12px; background: #0f172a; border: 1px solid #6B4423; border-radius: 8px; color: #f1f5f9; font-size: 14px; }
        .form-group input:focus, .form-group select:focus { outline: none; border-color: #ffffff; box-shadow: 0 0 0 3px rgba(255,255,255,0.2); }
        .form-buttons { display: flex; gap: 12px; margin-top: 24px; }
        .btn { flex: 1; padding: 12px 24px; border: none; border-radius: 8px; font-size: 14px; font-weight: 600; cursor: pointer; }
        .btn-save { background: #ffffff; color: #6B4423; }
        .btn-cancel { background: #6B4423; color: #ffffff; }
        .alert { padding: 16px 20px; border-radius: 8px; margin-bottom: 20px; }
        .alert-success { background: rgba(22, 163, 74, 0.1); border: 1px solid #16a34a; color: #16a34a; }
        .alert-danger { background: rgba(107, 68, 35, 0.2); border: 1px solid #6B4423; color: #e8c4a0; }
        .grid-view { width: 100%; border-collapse: collapse; }
        .grid-view th { background: #0f172a; padding: 14px 12px; text-align: left; font-size: 12px; color: #64748b; border-bottom: 1px solid #6B4423; }
        .grid-view td { padding: 14px 12px; border-bottom: 1px solid #6B4423; color: #e2e8f0; }
        .grid-view .action-cell { white-space: nowrap; }
        .grid-view .btn-edit, .grid-view .btn-delete { display: inline-block; text-decoration: none; padding: 6px 12px; border-radius: 6px; font-size: 12px; font-weight: 500; cursor: pointer; margin-right: 6px; border: 1px solid transparent; transition: opacity 0.2s; }
        .grid-view .btn-edit:hover, .grid-view .btn-delete:hover { opacity: 0.9; text-decoration: none; }
        .grid-view .btn-edit { background: #ffffff; color: #6B4423; border-color: #6B4423; }
        .grid-view .btn-edit:hover { background: #f1f5f9; color: #5a3a1e; }
        .grid-view .btn-delete { background: #6B4423; color: #ffffff; }
        .grid-view .btn-delete:hover { background: #8B5A2B; color: #ffffff; }
        .page-header { display: flex; align-items: center; margin-bottom: 32px; }
        .data-section { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 24px; }
        .data-section h3 { font-size: 20px; font-weight: 600; color: #ffffff; margin-bottom: 20px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-header">
        <h1 style="font-size: 32px; font-weight: 700; color: #f1f5f9;">&#127917; Hall Management</h1>
    </div>

    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>

    <div class="form-container">
        <h2 style="color: #f1f5f9; margin-bottom: 24px;"><asp:Label ID="lblFormTitle" runat="server">Add New Hall</asp:Label></h2>

        <div class="form-group">
            <label>Hall Name *</label>
            <asp:TextBox ID="txtHallName" runat="server" placeholder="e.g., Hall A - IMAX"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Capacity *</label>
            <asp:TextBox ID="txtCapacity" runat="server" TextMode="Number" placeholder="Number of seats"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Hall Type</label>
            <asp:DropDownList ID="ddlHallType" runat="server">
                <asp:ListItem Value="">-- Select --</asp:ListItem>
                <asp:ListItem Value="IMAX">IMAX</asp:ListItem>
                <asp:ListItem Value="4DX">4DX</asp:ListItem>
                <asp:ListItem Value="Dolby Atmos">Dolby Atmos</asp:ListItem>
                <asp:ListItem Value="Standard">Standard</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <label>Screen Size *</label>
            <asp:DropDownList ID="ddlScreenSize" runat="server">
                <asp:ListItem Value="">-- Select --</asp:ListItem>
                <asp:ListItem Value="Large">Large</asp:ListItem>
                <asp:ListItem Value="Medium">Medium</asp:ListItem>
                <asp:ListItem Value="Small">Small</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <label>Theatre *</label>
            <asp:DropDownList ID="ddlTheatre" runat="server"></asp:DropDownList>
        </div>

        <div class="form-buttons">
            <asp:Button ID="btnSave" runat="server" Text="Save Hall" CssClass="btn btn-save" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-cancel" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="data-section">
        <h3>All Halls</h3>
        <asp:GridView ID="gvHalls" runat="server" CssClass="grid-view" AllowPaging="true" PageSize="20" 
            OnPageIndexChanging="gvHalls_PageIndexChanging" OnRowCommand="gvHalls_RowCommand" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="HallID" HeaderText="ID" />
                <asp:BoundField DataField="HallName" HeaderText="Hall Name" />
                <asp:BoundField DataField="HallCapacity" HeaderText="Capacity" />
                <asp:BoundField DataField="HallType" HeaderText="Type" />
                <asp:BoundField DataField="ScreenSize" HeaderText="Screen" />
                <asp:BoundField DataField="TheatreName" HeaderText="Theatre" />
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="action-cell" ItemStyle-Width="140px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditHall" CommandArgument='<%# Eval("HallID") %>' CssClass="btn-edit">Edit</asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteHall" CommandArgument='<%# Eval("HallID") %>' CssClass="btn-delete" OnClientClick="return confirm('Delete this hall?')">Delete</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle HorizontalAlign="Center" ForeColor="#f1f5f9" />
            <EmptyDataTemplate><tr><td colspan="7" style="text-align:center;padding:20px;color:#94a3b8;">No halls found</td></tr></EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>
