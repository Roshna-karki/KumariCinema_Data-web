<%@ Page Title="Theatre Management" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="TheatreDetails.aspx.cs" Inherits="KumariCinema.TheatreDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-container {
            background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
            border: 1px solid #6B4423;
            border-radius: 12px;
            padding: 32px;
            margin-bottom: 32px;
            max-width: 600px;
        }
        .form-group {
            margin-bottom: 20px;
        }
        .form-group label {
            display: block;
            margin-bottom: 8px;
            color: #ffffff;
            font-weight: 500;
        }
        .form-group input,
        .form-group textarea {
            width: 100%;
            padding: 12px;
            background: #0f172a;
            border: 1px solid #6B4423;
            border-radius: 8px;
            color: #f1f5f9;
            font-size: 14px;
            transition: border-color 0.3s ease;
        }
        .form-group input:focus,
        .form-group textarea:focus {
            outline: none;
            border-color: #ffffff;
            box-shadow: 0 0 0 3px rgba(255,255,255,0.2);
        }
        .form-buttons {
            display: flex;
            gap: 12px;
            margin-top: 24px;
        }
        .btn {
            flex: 1;
            padding: 12px 24px;
            border: none;
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
        }
        .btn-save {
            background: #ffffff;
            color: #6B4423;
            font-weight: 600;
        }
        .btn-save:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 16px rgba(255,255,255,0.2);
        }
        .btn-cancel {
            background: #6B4423;
            color: #ffffff;
        }
        .btn-cancel:hover {
            background: #475569;
        }
        .alert {
            padding: 16px 20px;
            border-radius: 8px;
            margin-bottom: 20px;
            animation: slideIn 0.3s ease;
        }
        @keyframes slideIn {
            from { opacity: 0; transform: translateY(-10px); }
            to { opacity: 1; transform: translateY(0); }
        }
        .alert-success {
            background: rgba(22, 163, 74, 0.1);
            border: 1px solid #16a34a;
            color: #16a34a;
        }
        .alert-danger {
            background: rgba(107, 68, 35, 0.2);
            border: 1px solid #6B4423;
            color: #e8c4a0;
        }
        .grid-view {
            width: 100%;
            border-collapse: collapse;
        }
        .grid-view th {
            background: #0f172a;
            padding: 14px 12px;
            text-align: left;
            font-size: 12px;
            color: #64748b;
            text-transform: uppercase;
            font-weight: 600;
            border-bottom: 1px solid #6B4423;
        }
        .grid-view td {
            padding: 14px 12px;
            border-bottom: 1px solid #6B4423;
            color: #e2e8f0;
            font-size: 14px;
        }
        .grid-view tr:hover {
            background: #0f172a;
        }
        .grid-view .action-cell { white-space: nowrap; }
        .grid-view .btn-edit, .grid-view .btn-delete { display: inline-block; text-decoration: none; padding: 6px 12px; border-radius: 6px; font-size: 12px; font-weight: 500; cursor: pointer; margin-right: 6px; border: 1px solid transparent; transition: opacity 0.2s; }
        .grid-view .btn-edit:hover, .grid-view .btn-delete:hover { opacity: 0.9; text-decoration: none; }
        .grid-view .btn-edit { background: #ffffff; color: #6B4423; border-color: #6B4423; }
        .grid-view .btn-edit:hover { background: #f1f5f9; color: #5a3a1e; }
        .grid-view .btn-delete { background: #6B4423; color: #ffffff; }
        .grid-view .btn-delete:hover { background: #8B5A2B; color: #ffffff; }
        .page-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 32px;
        }
        .page-header h1 {
            font-size: 32px;
            font-weight: 700;
            color: #ffffff;
        }
        .data-section {
            background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
            border: 1px solid #6B4423;
            border-radius: 12px;
            padding: 24px;
        }
        .data-section h3 {
            font-size: 20px;
            font-weight: 600;
            color: #ffffff;
            margin-bottom: 20px;
        }
        .pager {
            color: #f1f5f9;
            text-align: center;
            padding: 12px;
            font-size: 13px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-header">
        <h1>&#127917; Theatre Management</h1>
    </div>

    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>

    <div class="form-container">
        <h2 style="color: #f1f5f9; margin-bottom: 24px; font-size: 20px;">
            <asp:Label ID="lblFormTitle" runat="server">Add New Theatre</asp:Label>
        </h2>

        <div class="form-group">
            <label>Theatre Name *</label>
            <asp:TextBox ID="txtTheatreName" runat="server" MaxLength="100" placeholder="Enter theatre name"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>City *</label>
            <asp:TextBox ID="txtCity" runat="server" MaxLength="50" placeholder="Enter city name"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Address</label>
            <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="3" placeholder="Theatre address"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Phone Number *</label>
            <asp:TextBox ID="txtContactNumber" runat="server" MaxLength="20" placeholder="10-15 digit phone number"></asp:TextBox>
        </div>

        <div class="form-buttons">
            <asp:Button ID="btnSave" runat="server" Text="Save Theatre" CssClass="btn btn-save" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-cancel" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="data-section">
        <h3>All Theatres</h3>
        <asp:GridView ID="gvTheatres" runat="server" CssClass="grid-view" AllowPaging="true" PageSize="20" 
            OnPageIndexChanging="gvTheatres_PageIndexChanging" OnRowCommand="gvTheatres_RowCommand" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="TheatreID" HeaderText="ID" />
                <asp:BoundField DataField="TheatreName" HeaderText="Theatre Name" />
                <asp:BoundField DataField="City" HeaderText="City" />
                <asp:BoundField DataField="TheatrePhoneNumber" HeaderText="Contact" />
                <asp:BoundField DataField="TheatreAddress" HeaderText="Address" />
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="action-cell" ItemStyle-Width="140px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditTheatre" CommandArgument='<%# Eval("TheatreID") %>' CssClass="btn-edit">Edit</asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteTheatre" CommandArgument='<%# Eval("TheatreID") %>' CssClass="btn-delete" OnClientClick="return confirm('Delete this theatre?')">Delete</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pager" />
            <EmptyDataTemplate>
                <tr><td colspan="6" style="text-align: center; padding: 20px; color: #94a3b8;">No theatres found</td></tr>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>
