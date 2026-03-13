<%@ Page Title="Movie Management" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="MovieDetails.aspx.cs" Inherits="KumariCinema.MovieDetails" %>

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
        .form-group select,
        .form-group textarea {
            width: 100%;
            padding: 12px;
            background: #0f172a;
            border: 1px solid #6B4423;
            border-radius: 8px;
            color: #ffffff;
            font-size: 14px;
            transition: border-color 0.3s ease;
        }

        .form-group input:focus,
        .form-group select:focus,
        .form-group textarea:focus {
            outline: none;
            border-color: #ffffff;
            box-shadow: 0 0 0 3px rgba(255,255,255,0.2);
        }

        .form-group textarea {
            resize: vertical;
            min-height: 100px;
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
            background: #8B5A2B;
        }

        .alert {
            padding: 16px 20px;
            border-radius: 8px;
            margin-bottom: 20px;
            animation: slideIn 0.3s ease;
        }

        @keyframes slideIn {
            from {
                opacity: 0;
                transform: translateY(-10px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
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

        .search-box {
            margin-bottom: 20px;
        }

        .search-box input {
            max-width: 400px;
        }

        .pager {
            color: #ffffff;
            text-align: center;
            padding: 12px;
            font-size: 13px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-header">
        <h1><i class="fa-solid fa-clapperboard" style="margin-right: 10px; color: #6B4423;" aria-hidden="true"></i>Movie Management</h1>
    </div>

    <!-- MESSAGE -->
    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>

    <!-- FORM SECTION -->
    <div class="form-container">
        <h2 style="color: #ffffff; margin-bottom: 24px; font-size: 20px;">
            <asp:Label ID="lblFormTitle" runat="server">Add New Movie</asp:Label>
        </h2>

        <div class="form-group">
            <label>Movie Title *</label>
            <asp:TextBox ID="txtTitle" runat="server" MaxLength="100" placeholder="Enter movie title"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Duration (minutes) *</label>
            <asp:TextBox ID="txtDuration" runat="server" TextMode="Number" Min="60" Max="300" placeholder="e.g., 120"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>Genre *</label>
            <asp:DropDownList ID="ddlGenre" runat="server">
                <asp:ListItem Value="">-- Select Genre --</asp:ListItem>
                <asp:ListItem Value="Action">Action</asp:ListItem>
                <asp:ListItem Value="Drama">Drama</asp:ListItem>
                <asp:ListItem Value="Comedy">Comedy</asp:ListItem>
                <asp:ListItem Value="Thriller">Thriller</asp:ListItem>
                <asp:ListItem Value="Horror">Horror</asp:ListItem>
                <asp:ListItem Value="Sci-Fi">Sci-Fi</asp:ListItem>
                <asp:ListItem Value="Romance">Romance</asp:ListItem>
                <asp:ListItem Value="Animation">Animation</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <label>Language *</label>
            <asp:DropDownList ID="ddlLanguage" runat="server">
                <asp:ListItem Value="">-- Select Language --</asp:ListItem>
                <asp:ListItem Value="English">English</asp:ListItem>
                <asp:ListItem Value="Hindi">Hindi</asp:ListItem>
                <asp:ListItem Value="Tamil">Tamil</asp:ListItem>
                <asp:ListItem Value="Telugu">Telugu</asp:ListItem>
                <asp:ListItem Value="Kannada">Kannada</asp:ListItem>
                <asp:ListItem Value="Marathi">Marathi</asp:ListItem>
                <asp:ListItem Value="Gujarati">Gujarati</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <label>Release Date *</label>
            <asp:TextBox ID="txtReleaseDate" runat="server" TextMode="Date"></asp:TextBox>
        </div>

        <div class="form-buttons">
            <asp:Button ID="btnSave" runat="server" Text="Save Movie" CssClass="btn btn-save" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-cancel" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <!-- DATA SECTION -->
    <div class="data-section">
        <h3><i class="fa-solid fa-film" style="margin-right: 8px; color: #94a3b8;" aria-hidden="true"></i>All Movies</h3>

        <div class="search-box">
            <asp:TextBox ID="txtSearch" runat="server" placeholder="Search by title or genre..." Width="400px" 
                Style="padding: 10px; background: #0f172a; border: 1px solid #6B4423; border-radius: 8px; color: #ffffff;"></asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-save" Style="width: auto; padding: 10px 20px; margin-left: 10px;" OnClick="btnSearch_Click" />
            <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-cancel" Style="width: auto; padding: 10px 20px; margin-left: 5px;" OnClick="btnClearSearch_Click" CausesValidation="false" />
        </div>

        <asp:GridView ID="gvMovies" runat="server" CssClass="grid-view" AllowPaging="true" PageSize="20" 
            OnPageIndexChanging="gvMovies_PageIndexChanging" OnRowCommand="gvMovies_RowCommand" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="MovieID" HeaderText="ID" />
                <asp:BoundField DataField="Title" HeaderText="Title" />
                <asp:BoundField DataField="Genre" HeaderText="Genre" />
                <asp:BoundField DataField="Language" HeaderText="Language" />
                <asp:BoundField DataField="Duration" HeaderText="Duration" />
                <asp:BoundField DataField="ReleaseDate" HeaderText="Release Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="action-cell" ItemStyle-Width="140px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditMovie" CommandArgument='<%# Eval("MovieID") %>' CssClass="btn-edit">Edit</asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteMovie" CommandArgument='<%# Eval("MovieID") %>' CssClass="btn-delete" OnClientClick="return confirm('Delete this movie?')">Delete</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pager" />
            <EmptyDataTemplate>
                <tr><td colspan="7" style="text-align: center; padding: 20px; color: #94a3b8;">No movies found</td></tr>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>
