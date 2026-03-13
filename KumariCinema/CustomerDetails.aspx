<%@ Page Title="Customer Management" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="CustomerDetails.aspx.cs" Inherits="KumariCinema.CustomerDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-container { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 32px; margin-bottom: 32px; max-width: 600px; }
        .form-group { margin-bottom: 20px; }
        .form-group label { display: block; margin-bottom: 8px; color: #ffffff; font-weight: 500; }
        .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 12px; background: #0f172a; border: 1px solid #6B4423; border-radius: 8px; color: #ffffff; }
        .form-group input:focus, .form-group select:focus, .form-group textarea:focus { outline: none; border-color: #ffffff; box-shadow: 0 0 0 2px rgba(255,255,255,0.2); }
        .btn-save { flex: 1; padding: 12px; background: #ffffff; color: #6B4423; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; }
        .btn-cancel { flex: 1; padding: 12px; background: #6B4423; color: #ffffff; border: none; border-radius: 8px; cursor: pointer; }
        .form-buttons { display: flex; gap: 12px; margin-top: 24px; }
        .alert { padding: 16px; border-radius: 8px; margin-bottom: 20px; }
        .alert-success { background: rgba(22, 163, 74, 0.1); border: 1px solid #16a34a; color: #16a34a; }
        .alert-danger { background: rgba(107, 68, 35, 0.2); border: 1px solid #6B4423; color: #e8c4a0; }
        .data-section { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 24px; }
        .grid-view { width: 100%; border-collapse: collapse; }
        .grid-view th { background: #0f172a; padding: 12px; text-align: left; font-size: 11px; color: #64748b; border-bottom: 1px solid #6B4423; }
        .grid-view td { padding: 12px; border-bottom: 1px solid #6B4423; color: #e2e8f0; font-size: 13px; }
        .grid-view .action-cell { white-space: nowrap; }
        .grid-view .btn-edit, .grid-view .btn-delete { display: inline-block; text-decoration: none; padding: 6px 12px; border-radius: 6px; font-size: 12px; font-weight: 500; cursor: pointer; margin-right: 6px; border: 1px solid transparent; transition: opacity 0.2s; }
        .grid-view .btn-edit:hover, .grid-view .btn-delete:hover { opacity: 0.9; text-decoration: none; }
        .grid-view .btn-edit { background: #ffffff; color: #6B4423; border-color: #6B4423; }
        .grid-view .btn-edit:hover { background: #f1f5f9; color: #5a3a1e; }
        .grid-view .btn-delete { background: #6B4423; color: #ffffff; }
        .grid-view .btn-delete:hover { background: #8B5A2B; color: #ffffff; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size: 32px; font-weight: 700; color: #ffffff; margin-bottom: 32px;"><i class="fa-solid fa-users" style="margin-right: 10px; color: #6B4423;" aria-hidden="true"></i>Customer Management</h1>
    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>

    <div class="form-container">
        <h2 style="color: #ffffff; margin-bottom: 24px;"><asp:Label ID="lblFormTitle" runat="server">Register New Customer</asp:Label></h2>
        <div class="form-group">
            <label>Full Name *</label>
            <asp:TextBox ID="txtName" runat="server" MaxLength="100" placeholder="Enter customer name"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Email *</label>
            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="customer@email.com"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Phone Number *</label>
            <asp:TextBox ID="txtPhone" runat="server" MaxLength="15" placeholder="Phone number"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Address</label>
            <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="3" placeholder="Address"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Date of Birth</label>
            <asp:TextBox ID="txtDOB" runat="server" TextMode="Date"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>Gender</label>
            <asp:DropDownList ID="ddlGender" runat="server">
                <asp:ListItem Value="">-- Select --</asp:ListItem>
                <asp:ListItem Value="Male">Male</asp:ListItem>
                <asp:ListItem Value="Female">Female</asp:ListItem>
                <asp:ListItem Value="Other">Other</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Membership Tier</label>
            <asp:DropDownList ID="ddlMembership" runat="server">
                <asp:ListItem Value="">-- Select --</asp:ListItem>
                <asp:ListItem Value="Standard">Standard</asp:ListItem>
                <asp:ListItem Value="Silver">Silver</asp:ListItem>
                <asp:ListItem Value="Gold">Gold</asp:ListItem>
                <asp:ListItem Value="Platinum">Platinum</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-buttons">
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn-save" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-cancel" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="data-section">
        <h3 style="font-size: 18px; font-weight: 600; color: #ffffff; margin-bottom: 16px;">All Customers</h3>
        <asp:GridView ID="gvCustomers" runat="server" CssClass="grid-view" AllowPaging="true" PageSize="20" 
            OnPageIndexChanging="gvCustomers_PageIndexChanging" OnRowCommand="gvCustomers_RowCommand" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="UserID" HeaderText="ID" />
                <asp:BoundField DataField="UserName" HeaderText="Name" />
                <asp:BoundField DataField="UserEmail" HeaderText="Email" />
                <asp:BoundField DataField="PhoneNumber" HeaderText="Phone" />
                <asp:BoundField DataField="UserAddress" HeaderText="Address" />
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="action-cell" ItemStyle-Width="140px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditCustomer" CommandArgument='<%# Eval("UserID") %>' CssClass="btn-edit">Edit</asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteCustomer" CommandArgument='<%# Eval("UserID") %>' CssClass="btn-delete" OnClientClick="return confirm('Delete this customer?')">Delete</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle HorizontalAlign="Center" />
            <EmptyDataTemplate><tr><td colspan="6" style="text-align:center;padding:20px;color:#94a3b8;">No customers</td></tr></EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>
