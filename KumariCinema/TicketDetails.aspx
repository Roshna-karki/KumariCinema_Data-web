<%@ Page Title="Ticket Management" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="TicketDetails.aspx.cs" Inherits="KumariCinema.TicketDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .data-section { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 24px; }
        .alert { padding: 16px; border-radius: 8px; margin-bottom: 20px; }
        .alert-success { background: rgba(22, 163, 74, 0.1); border: 1px solid #16a34a; color: #16a34a; }
        .alert-danger { background: rgba(107, 68, 35, 0.2); border: 1px solid #6B4423; color: #e8c4a0; }
        .grid-view { width: 100%; border-collapse: collapse; }
        .grid-view th { background: #0f172a; padding: 12px; text-align: left; font-size: 11px; color: #64748b; border-bottom: 1px solid #6B4423; }
        .grid-view td { padding: 12px; border-bottom: 1px solid #6B4423; color: #e2e8f0; font-size: 13px; }
        .grid-view .action-cell { white-space: nowrap; }
        .grid-view .btn-delete { display: inline-block; text-decoration: none; padding: 6px 12px; border-radius: 6px; font-size: 12px; font-weight: 500; cursor: pointer; background: #6B4423; color: #ffffff; border: 1px solid transparent; transition: opacity 0.2s; }
        .grid-view .btn-delete:hover { opacity: 0.9; text-decoration: none; background: #8B5A2B; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size: 32px; font-weight: 700; color: #ffffff; margin-bottom: 32px;"><i class="fa-solid fa-ticket" style="margin-right: 10px; color: #6B4423;" aria-hidden="true"></i>Ticket Management</h1>
    <p style="color: #94a3b8; margin-bottom: 24px;">Tickets are created when a booking is confirmed. Below are all tickets with booking, theatre, hall, seat and show details.</p>
    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>

    <div class="data-section">
        <h3 style="font-size: 18px; font-weight: 600; color: #ffffff; margin-bottom: 16px;">All Tickets</h3>
        <asp:GridView ID="gvTickets" runat="server" CssClass="grid-view" AllowPaging="true" PageSize="20"
            OnPageIndexChanging="gvTickets_PageIndexChanging" OnRowCommand="gvTickets_RowCommand" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="TicketID" HeaderText="ID" />
                <asp:BoundField DataField="TicketNumber" HeaderText="Ticket #" />
                <asp:BoundField DataField="ConfirmationCode" HeaderText="Booking #" />
                <asp:BoundField DataField="UserName" HeaderText="Customer" />
                <asp:BoundField DataField="TheatreName" HeaderText="Theatre" />
                <asp:BoundField DataField="HallName" HeaderText="Hall" />
                <asp:BoundField DataField="Title" HeaderText="Movie" />
                <asp:BoundField DataField="ShowDate" HeaderText="Show date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="ShowTime" HeaderText="Time" />
                <asp:BoundField DataField="SeatInfo" HeaderText="Seat" />
                <asp:BoundField DataField="TicketStatus" HeaderText="Status" />
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="action-cell" ItemStyle-Width="100px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="CancelTicket" CommandArgument='<%# Eval("TicketID") %>' CssClass="btn-delete" OnClientClick="return confirm('Cancel this ticket?')">Cancel</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle HorizontalAlign="Center" />
            <EmptyDataTemplate><tr><td colspan="11" style="text-align:center;padding:20px;color:#94a3b8;">No tickets. Tickets are created when you confirm a booking.</td></tr></EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>
