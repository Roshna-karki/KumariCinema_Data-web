<%@ Page Title="Seat Management" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="SeatDetails.aspx.cs" Inherits="KumariCinema.SeatDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-container { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 32px; margin-bottom: 32px; max-width: 600px; }
        .form-group { margin-bottom: 20px; }
        .form-group label { display: block; margin-bottom: 8px; color: #ffffff; font-weight: 500; }
        .form-group input, .form-group select { width: 100%; padding: 12px; background: #0f172a; border: 1px solid #6B4423; border-radius: 8px; color: #ffffff; }
        .form-group input:checked { accent-color: #6B4423; }
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
        .grid-view .btn-delete { display: inline-block; text-decoration: none; padding: 6px 12px; border-radius: 6px; font-size: 12px; font-weight: 500; cursor: pointer; background: #6B4423; color: #ffffff; border: 1px solid transparent; transition: opacity 0.2s; }
        .grid-view .btn-delete:hover { opacity: 0.9; text-decoration: none; background: #8B5A2B; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size: 32px; font-weight: 700; color: #ffffff; margin-bottom: 32px;">&#128186; Seat Management</h1>
    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>

    <div class="form-container">
        <h2 style="color: #ffffff; margin-bottom: 24px;">Add New Seat</h2>
        <div class="form-group">
            <label>Seat Row *</label>
            <asp:DropDownList ID="ddlRow" runat="server">
                <asp:ListItem Value="">-- Select --</asp:ListItem>
                <asp:ListItem Value="A">A</asp:ListItem>
                <asp:ListItem Value="B">B</asp:ListItem>
                <asp:ListItem Value="C">C</asp:ListItem>
                <asp:ListItem Value="D">D</asp:ListItem>
                <asp:ListItem Value="E">E</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Seat Number *</label>
            <asp:TextBox ID="txtSeatNumber" runat="server" TextMode="Number" Min="1" Max="50"></asp:TextBox>
        </div>
        <div class="form-group">
            <label style="display: flex; align-items: center; gap: 10px;">
                <asp:CheckBox ID="chkAvailable" runat="server" /> Available
            </label>
        </div>
        <div class="form-buttons">
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn-save" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-cancel" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="data-section">
        <h3 style="font-size: 18px; font-weight: 600; color: #ffffff; margin-bottom: 16px;">All Seats</h3>
        <asp:GridView ID="gvSeats" runat="server" CssClass="grid-view" AllowPaging="true" PageSize="20" 
            OnPageIndexChanging="gvSeats_PageIndexChanging" OnRowCommand="gvSeats_RowCommand" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="SeatID" HeaderText="ID" />
                <asp:BoundField DataField="SeatRow" HeaderText="Row" />
                <asp:BoundField DataField="SeatNumber" HeaderText="Number" />
                <asp:BoundField DataField="IsAvailable" HeaderText="Available" />
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="action-cell" ItemStyle-Width="100px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteSeat" CommandArgument='<%# Eval("SeatID") %>' CssClass="btn-delete" OnClientClick="return confirm('Delete this seat?')">Delete</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle HorizontalAlign="Center" />
            <EmptyDataTemplate><tr><td colspan="5" style="text-align:center;padding:20px;color:#94a3b8;">No seats</td></tr></EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>
