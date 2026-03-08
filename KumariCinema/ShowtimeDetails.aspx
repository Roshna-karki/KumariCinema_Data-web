<%@ Page Title="Showtime Management" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="ShowtimeDetails.aspx.cs" Inherits="KumariCinema.ShowtimeDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-container { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 32px; margin-bottom: 32px; max-width: 600px; }
        .form-group { margin-bottom: 20px; }
        .form-group label { display: block; margin-bottom: 8px; color: #ffffff; font-weight: 500; }
        .form-group input, .form-group select { width: 100%; padding: 12px; background: #0f172a; border: 1px solid #6B4423; border-radius: 8px; color: #f1f5f9; font-size: 14px; }
        .form-group input:focus, .form-group select:focus { outline: none; border-color: #ffffff; }
        .form-buttons { display: flex; gap: 12px; margin-top: 24px; }
        .btn { flex: 1; padding: 12px 24px; border: none; border-radius: 8px; font-size: 14px; font-weight: 600; cursor: pointer; }
        .btn-save { background: #ffffff; color: #6B4423; }
        .btn-cancel { background: #6B4423; color: #ffffff; }
        .alert { padding: 16px 20px; border-radius: 8px; margin-bottom: 20px; }
        .alert-success { background: rgba(22, 163, 74, 0.1); border: 1px solid #16a34a; color: #16a34a; }
        .alert-danger { background: rgba(107, 68, 35, 0.2); border: 1px solid #6B4423; color: #e8c4a0; }
        .grid-view { width: 100%; border-collapse: collapse; }
        .grid-view th { background: #0f172a; padding: 12px; text-align: left; font-size: 11px; color: #64748b; border-bottom: 1px solid #6B4423; }
        .grid-view td { padding: 12px; border-bottom: 1px solid #6B4423; color: #e2e8f0; font-size: 13px; }
        .grid-view .action-cell { white-space: nowrap; }
        .grid-view .btn-edit,
        .grid-view .btn-delete { display: inline-block; text-decoration: none; padding: 6px 12px; border-radius: 6px; font-size: 12px; font-weight: 500; cursor: pointer; margin-right: 6px; border: 1px solid transparent; transition: opacity 0.2s; }
        .grid-view .btn-edit:hover,
        .grid-view .btn-delete:hover { opacity: 0.9; text-decoration: none; }
        .grid-view .btn-edit { background: #ffffff; color: #6B4423; border-color: #6B4423; }
        .grid-view .btn-edit:hover { background: #f1f5f9; color: #5a3a1e; }
        .grid-view .btn-delete { background: #6B4423; color: #ffffff; }
        .grid-view .btn-delete:hover { background: #8B5A2B; color: #ffffff; }
        .data-section { background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%); border: 1px solid #6B4423; border-radius: 12px; padding: 24px; margin-top: 32px; }
        .data-section h3 { font-size: 18px; font-weight: 600; color: #ffffff; margin-bottom: 16px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 style="font-size: 32px; font-weight: 700; color: #f1f5f9; margin-bottom: 32px;">&#128337; Showtime Management</h1>

    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>

    <div class="form-container">
        <h2 style="color: #f1f5f9; margin-bottom: 24px;"><asp:Label ID="lblFormTitle" runat="server">Add New Show</asp:Label></h2>

        <div class="form-group">
            <label>Show Date *</label>
            <asp:TextBox ID="txtShowDate" runat="server" TextMode="Date"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Show Time *</label>
            <asp:DropDownList ID="ddlShowTime" runat="server">
                <asp:ListItem Value="">-- Select Time --</asp:ListItem>
                <asp:ListItem Value="09:00">9:00 AM</asp:ListItem>
                <asp:ListItem Value="12:00">12:00 PM</asp:ListItem>
                <asp:ListItem Value="15:00">3:00 PM</asp:ListItem>
                <asp:ListItem Value="18:00">6:00 PM</asp:ListItem>
                <asp:ListItem Value="21:00">9:00 PM</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <label>Movie *</label>
            <asp:DropDownList ID="ddlMovie" runat="server"></asp:DropDownList>
        </div>

        <div class="form-group">
            <label>Hall *</label>
            <asp:DropDownList ID="ddlHall" runat="server"></asp:DropDownList>
        </div>

        <div class="form-buttons">
            <asp:Button ID="btnSave" runat="server" Text="Save Show" CssClass="btn btn-save" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-cancel" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="data-section">
        <h3>All Showtimes</h3>
        <asp:GridView ID="gvShows" runat="server" CssClass="grid-view" AllowPaging="true" PageSize="20" 
            OnPageIndexChanging="gvShows_PageIndexChanging" OnRowCommand="gvShows_RowCommand" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="ShowID" HeaderText="ID" ItemStyle-Width="40px" />
                <asp:BoundField DataField="MovieTitle" HeaderText="Movie" />
                <asp:BoundField DataField="ShowDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" ItemStyle-Width="100px" />
                <asp:BoundField DataField="ShowTime" HeaderText="Time" ItemStyle-Width="70px" />
                <asp:BoundField DataField="HallName" HeaderText="Hall" />
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="action-cell" ItemStyle-Width="140px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditShow" CommandArgument='<%# Eval("ShowID") %>' CssClass="btn-edit">Edit</asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteShow" CommandArgument='<%# Eval("ShowID") %>' CssClass="btn-delete" OnClientClick="return confirm('Delete this show?')">Delete</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle HorizontalAlign="Center" />
            <EmptyDataTemplate><tr><td colspan="6" style="text-align:center;padding:20px;color:#94a3b8;">No shows</td></tr></EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>
