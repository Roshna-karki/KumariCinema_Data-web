<%@ Page Title="Booking Management" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="BookingDetails.aspx.cs" Inherits="KumariCinema.BookingDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-container { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 32px; margin-bottom: 32px; max-width: 700px; }
        .form-group { margin-bottom: 20px; }
        .form-group label { display: block; margin-bottom: 8px; color: #ffffff; font-weight: 500; }
        .form-group input, .form-group select { width: 100%; padding: 12px; background: #0f172a; border: 1px solid #6B4423; border-radius: 8px; color: #ffffff; }
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
        .seat-panel { border: 1px solid #6B4423; border-radius: 8px; padding: 16px; margin-top: 12px; max-height: 220px; overflow-y: auto; }
        .seat-grid { display: flex; flex-wrap: wrap; gap: 8px; }
        .seat-item { padding: 8px 12px; background: #0f172a; border: 1px solid #6B4423; border-radius: 6px; color: #e2e8f0; cursor: pointer; }
        .seat-item.selected { background: #6B4423; color: #fff; }
        .price-summary { margin-top: 20px; padding: 20px 24px; background: linear-gradient(135deg, #1a2332 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 10px; }
        .price-summary .price-summary-title { font-size: 14px; font-weight: 600; color: #94a3b8; margin-bottom: 14px; letter-spacing: 0.5px; }
        .price-summary .price-row { display: flex; justify-content: space-between; align-items: center; padding: 10px 0; border-bottom: 1px solid rgba(107, 68, 35, 0.3); }
        .price-summary .price-row:last-child { border-bottom: none; padding-bottom: 0; }
        .price-summary .price-row.total-row { margin-top: 8px; padding-top: 14px; border-top: 2px solid #6B4423; }
        .price-summary .price-label { color: #cbd5e1; font-size: 14px; }
        .price-summary .price-value { color: #e8c4a0; font-weight: 700; font-size: 15px; }
        .price-summary .price-value.total-value { color: #ffffff; font-size: 18px; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size: 32px; font-weight: 700; color: #ffffff; margin-bottom: 32px;">&#128203; Booking Management</h1>
    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>

    <div class="form-container">
        <h2 style="color: #ffffff; margin-bottom: 24px;"><asp:Label ID="lblFormTitle" runat="server">New Booking</asp:Label></h2>
        <div class="form-group">
            <label>Theatre *</label>
            <asp:DropDownList ID="ddlTheatre" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlTheatre_SelectedIndexChanged"></asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Hall *</label>
            <asp:DropDownList ID="ddlHall" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlHall_SelectedIndexChanged"></asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Show (Movie &amp; Time) *</label>
            <asp:DropDownList ID="ddlShow" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlShow_SelectedIndexChanged"></asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Customer *</label>
            <asp:DropDownList ID="ddlCustomer" runat="server"></asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Number of tickets *</label>
            <asp:DropDownList ID="ddlNumTickets" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlNumTickets_SelectedIndexChanged">
                <asp:ListItem Value="">-- Select --</asp:ListItem>
                <asp:ListItem Value="1">1</asp:ListItem>
                <asp:ListItem Value="2">2</asp:ListItem>
                <asp:ListItem Value="3">3</asp:ListItem>
                <asp:ListItem Value="4">4</asp:ListItem>
                <asp:ListItem Value="5">5</asp:ListItem>
                <asp:ListItem Value="6">6</asp:ListItem>
                <asp:ListItem Value="7">7</asp:ListItem>
                <asp:ListItem Value="8">8</asp:ListItem>
                <asp:ListItem Value="9">9</asp:ListItem>
                <asp:ListItem Value="10">10</asp:ListItem>
            </asp:DropDownList>
        </div>
        <asp:Panel ID="pnlSeats" runat="server" Visible="false" CssClass="form-group">
            <label>Choose seats (select exactly <asp:Literal ID="litSeatCount" runat="server"></asp:Literal> seat(s)) *</label>
            <div class="seat-panel">
                <asp:CheckBoxList ID="chkSeats" runat="server" CssClass="seat-grid" RepeatDirection="Horizontal" RepeatLayout="Flow"></asp:CheckBoxList>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlPrice" runat="server" Visible="false" CssClass="price-summary">
            <div class="price-summary-title">Payment summary</div>
            <div class="price-row">
                <span class="price-label">Price per ticket</span>
                <asp:Label ID="lblPricePerTicket" runat="server" CssClass="price-value"></asp:Label>
            </div>
            <div class="price-row total-row">
                <span class="price-label">Total amount</span>
                <asp:Label ID="lblTotalAmount" runat="server" CssClass="price-value total-value"></asp:Label>
            </div>
        </asp:Panel>
        <div class="form-group" style="margin-top: 16px;">
            <label>Payment method *</label>
            <asp:DropDownList ID="ddlPaymentMethod" runat="server">
                <asp:ListItem Value="">-- Select --</asp:ListItem>
                <asp:ListItem Value="Credit Card">Credit Card</asp:ListItem>
                <asp:ListItem Value="Debit Card">Debit Card</asp:ListItem>
                <asp:ListItem Value="UPI">UPI</asp:ListItem>
                <asp:ListItem Value="Net Banking">Net Banking</asp:ListItem>
                <asp:ListItem Value="Wallet">Wallet</asp:ListItem>
                <asp:ListItem Value="Cash">Cash</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-buttons">
            <asp:Button ID="btnSave" runat="server" Text="Confirm Booking" CssClass="btn-save" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-cancel" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="data-section">
        <h3 style="font-size: 18px; font-weight: 600; color: #ffffff; margin-bottom: 16px;">All Bookings</h3>
        <asp:GridView ID="gvBookings" runat="server" CssClass="grid-view" AllowPaging="true" PageSize="20"
            OnPageIndexChanging="gvBookings_PageIndexChanging" OnRowCommand="gvBookings_RowCommand" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="BookingID" HeaderText="ID" />
                <asp:BoundField DataField="ConfirmationCode" HeaderText="Confirmation #" />
                <asp:BoundField DataField="UserName" HeaderText="Customer" />
                <asp:BoundField DataField="TheatreName" HeaderText="Theatre" />
                <asp:BoundField DataField="HallName" HeaderText="Hall" />
                <asp:BoundField DataField="Title" HeaderText="Movie" />
                <asp:BoundField DataField="ShowDate" HeaderText="Show Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="ShowTime" HeaderText="Time" />
                <asp:BoundField DataField="PaymentAmount" HeaderText="Amount" DataFormatString="{0:N2}" />
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="action-cell" ItemStyle-Width="140px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditBooking" CommandArgument='<%# Eval("BookingID") %>' CssClass="btn-edit">Edit</asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteBooking" CommandArgument='<%# Eval("BookingID") %>' CssClass="btn-delete" OnClientClick="return confirm('Cancel this booking?')">Cancel</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle HorizontalAlign="Center" />
            <EmptyDataTemplate><tr><td colspan="10" style="text-align:center;padding:20px;color:#94a3b8;">No bookings</td></tr></EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>
