<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="KumariCinema.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
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

        /* GRID LAYOUT */
        .grid {
            display: grid;
            gap: 20px;
            margin-bottom: 32px;
        }

        .grid-2 {
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
        }

        .grid-3 {
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
        }

        .grid-6 {
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        }

        /* STAT CARD */
        .stat-card {
            background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
            border: 1px solid #475569;
            border-radius: 12px;
            padding: 24px;
            position: relative;
            overflow: hidden;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.25);
        }

        .stat-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 3px;
            background: linear-gradient(90deg, #64748b, #6B4423, #8B5A2B);
        }

        .stat-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 12px 28px rgba(0, 0, 0, 0.35);
        }

        .stat-icon {
            width: 48px;
            height: 48px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 20px;
            margin-bottom: 16px;
        }

        .stat-icon.red { background: rgba(255,255,255,0.15); }
        .stat-icon.green { background: #16a34a1a; }
        .stat-icon.yellow { background: #eab3081a; }
        .stat-icon.pink { background: #ec48991a; }
        .stat-icon.blue { background: #2563eb1a; }
        .stat-icon.teal { background: #14b8a61a; }

        .stat-value {
            font-size: 36px;
            font-weight: 700;
            color: #ffffff;
            margin-bottom: 8px;
        }

        .stat-label {
            color: #94a3b8;
            font-size: 14px;
        }

        .stat-change {
            color: #16a34a;
            font-size: 13px;
            margin-top: 8px;
        }

        /* CHARTS */
        .charts-grid {
            display: grid;
            grid-template-columns: 1.5fr 1fr;
            gap: 20px;
            margin-bottom: 32px;
        }

        .chart-card {
            background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
            border: 1px solid #6B4423;
            border-radius: 12px;
            padding: 24px;
        }

        .chart-card h2 {
            font-size: 20px;
            font-weight: 600;
            color: #ffffff;
            margin-bottom: 20px;
        }

        .chart-container {
            position: relative;
            height: 300px;
        }

        /* DATA SECTIONS */
        .data-section {
            background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
            border: 1px solid #6B4423;
            border-radius: 12px;
            padding: 24px;
            margin-bottom: 20px;
        }

        .data-section h3 {
            font-size: 18px;
            font-weight: 600;
            color: #ffffff;
            margin-bottom: 16px;
            letter-spacing: 0.3px;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
        }

        .table th {
            background: #0f172a;
            padding: 12px;
            text-align: left;
            font-size: 12px;
            color: #64748b;
            text-transform: uppercase;
            font-weight: 600;
            border-bottom: 1px solid #6B4423;
        }

        .table td {
            padding: 12px;
            border-bottom: 1px solid #6B4423;
            color: #e2e8f0;
            font-size: 14px;
        }

        .table tr:hover {
            background: #0f172a;
        }

        .badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
        }

        .badge-success {
            background: rgba(22, 163, 74, 0.1);
            color: #16a34a;
        }

        .badge-warning {
            background: rgba(234, 179, 8, 0.1);
            color: #eab308;
        }

        .badge-danger {
            background: rgba(220, 38, 38, 0.1);
            color: #e8c4a0;
        }

        .badge-info {
            background: rgba(37, 99, 235, 0.1);
            color: #2563eb;
        }

        .badge-secondary {
            background: rgba(100, 116, 139, 0.1);
            color: #64748b;
        }

        .breakdown-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-top: 16px;
        }

        .breakdown-item {
            background: #0f172a;
            border: 1px solid #6B4423;
            border-radius: 8px;
            padding: 16px;
            text-align: center;
        }

        .breakdown-number {
            font-size: 28px;
            font-weight: 700;
            margin-bottom: 4px;
        }

        .breakdown-label {
            color: #94a3b8;
            font-size: 13px;
        }

        .breakdown-amount {
            font-size: 14px;
            margin-top: 8px;
            color: #16a34a;
        }

        @media (max-width: 1024px) {
            .charts-grid {
                grid-template-columns: 1fr;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-header">
        <h1><i class="fa-solid fa-chart-line" style="margin-right: 10px; color: #6B4423;" aria-hidden="true"></i>Dashboard</h1>
        <div>
            <asp:Label ID="lblRefreshTime" runat="server" ForeColor="#64748b" Font-Size="14"></asp:Label>
        </div>
    </div>

    <!-- STATISTICS CARDS (6 CARDS) -->
    <div class="grid grid-3">
        <div class="stat-card">
            <div class="stat-icon red"><i class="fa-solid fa-clapperboard" aria-hidden="true"></i></div>
            <div class="stat-value">
                <asp:Label ID="lblTotalMovies" runat="server">0</asp:Label>
            </div>
            <div class="stat-label">Total Movies</div>
            <div class="stat-change">
                <asp:Label ID="lblMoviesChange" runat="server">+0 this month</asp:Label>
            </div>
        </div>

        <div class="stat-card">
            <div class="stat-icon green"><i class="fa-solid fa-building" aria-hidden="true"></i></div>
            <div class="stat-value">
                <asp:Label ID="lblTotalTheatres" runat="server">0</asp:Label>
            </div>
            <div class="stat-label">Total Theatres</div>
            <div class="stat-change">
                <asp:Label ID="lblTheatresChange" runat="server">Locations</asp:Label>
            </div>
        </div>

        <div class="stat-card">
            <div class="stat-icon yellow"><i class="fa-solid fa-clock" aria-hidden="true"></i></div>
            <div class="stat-value">
                <asp:Label ID="lblShowsToday" runat="server">0</asp:Label>
            </div>
            <div class="stat-label">Showtimes Today</div>
            <div class="stat-change">
                <asp:Label ID="lblShowsChange" runat="server">Today</asp:Label>
            </div>
        </div>

        <div class="stat-card">
            <div class="stat-icon pink"><i class="fa-solid fa-ticket" aria-hidden="true"></i></div>
            <div class="stat-value">
                <asp:Label ID="lblTicketsSold" runat="server">0</asp:Label>
            </div>
            <div class="stat-label">Tickets Sold Today</div>
            <div class="stat-change">
                <asp:Label ID="lblTicketsChange" runat="server">Today</asp:Label>
            </div>
        </div>

        <div class="stat-card">
            <div class="stat-icon blue"><i class="fa-solid fa-users" aria-hidden="true"></i></div>
            <div class="stat-value">
                <asp:Label ID="lblTotalCustomers" runat="server">0</asp:Label>
            </div>
            <div class="stat-label">Total Customers</div>
            <div class="stat-change">
                <asp:Label ID="lblCustomersChange" runat="server">Members</asp:Label>
            </div>
        </div>

        <div class="stat-card">
            <div class="stat-icon teal"><i class="fa-solid fa-indian-rupee-sign" aria-hidden="true"></i></div>
            <div class="stat-value" style="font-size: 24px;">
                <asp:Label ID="lblRevenue" runat="server">Rs 0</asp:Label>
            </div>
            <div class="stat-label">Total Revenue</div>
            <div class="stat-change">
                <asp:Label ID="lblRevenueChange" runat="server">Completed</asp:Label>
            </div>
        </div>
    </div>

    <!-- CHARTS -->
    <div class="charts-grid">
        <div class="chart-card">
            <h2>Payment Status Distribution</h2>
            <div class="chart-container">
                <canvas id="paymentStatusChart"></canvas>
            </div>
            <asp:HiddenField ID="hdnPaymentData" runat="server" />
        </div>

        <div class="chart-card">
            <h2>Ticket Status Distribution</h2>
            <div class="chart-container">
                <canvas id="ticketStatusChart"></canvas>
            </div>
            <asp:HiddenField ID="hdnTicketData" runat="server" />
        </div>
    </div>

    <!-- PAYMENT BREAKDOWN -->
    <div class="data-section">
        <h3><i class="fa-solid fa-credit-card" style="margin-right: 8px; color: #94a3b8;" aria-hidden="true"></i>Payment Breakdown</h3>
        <div class="breakdown-grid">
            <div class="breakdown-item">
                <div class="breakdown-number" style="color: #16a34a;">
                    <asp:Label ID="lblCompletedPayments" runat="server">0</asp:Label>
                </div>
                <div class="breakdown-label">Completed Payments</div>
                <div class="breakdown-amount">
                    <asp:Label ID="lblCompletedAmount" runat="server">Rs 0</asp:Label>
                </div>
            </div>
            <div class="breakdown-item">
                <div class="breakdown-number" style="color: #eab308;">
                    <asp:Label ID="lblPendingPayments" runat="server">0</asp:Label>
                </div>
                <div class="breakdown-label">Pending Payments</div>
                <div class="breakdown-amount">
                    <asp:Label ID="lblPendingAmount" runat="server">Rs 0</asp:Label>
                </div>
            </div>
            <div class="breakdown-item">
                <div class="breakdown-number" style="color: #e8c4a0;">
                    <asp:Label ID="lblFailedPayments" runat="server">0</asp:Label>
                </div>
                <div class="breakdown-label">Failed Payments</div>
                <div class="breakdown-amount">
                    <asp:Label ID="lblFailedAmount" runat="server">Rs 0</asp:Label>
                </div>
            </div>
        </div>
    </div>

    <!-- TICKET BREAKDOWN -->
    <div class="data-section">
        <h3><i class="fa-solid fa-ticket" style="margin-right: 8px; color: #94a3b8;" aria-hidden="true"></i>Ticket Status</h3>
        <div class="breakdown-grid">
            <div class="breakdown-item">
                <div class="breakdown-number" style="color: #2563eb;">
                    <asp:Label ID="lblActiveTickets" runat="server">0</asp:Label>
                </div>
                <div class="breakdown-label">Active Tickets</div>
                <div class="breakdown-amount">Valid & Confirmed</div>
            </div>
            <div class="breakdown-item">
                <div class="breakdown-number" style="color: #16a34a;">
                    <asp:Label ID="lblUsedTickets" runat="server">0</asp:Label>
                </div>
                <div class="breakdown-label">Used Tickets</div>
                <div class="breakdown-amount">Shows Attended</div>
            </div>
            <div class="breakdown-item">
                <div class="breakdown-number" style="color: #e8c4a0;">
                    <asp:Label ID="lblCancelledTickets" runat="server">0</asp:Label>
                </div>
                <div class="breakdown-label">Cancelled Tickets</div>
                <div class="breakdown-amount">
                    <asp:Label ID="lblCancelledRate" runat="server">0%</asp:Label>
                </div>
            </div>
        </div>
    </div>

    <!-- NOW SHOWING -->
    <div class="data-section">
        <h3><i class="fa-solid fa-film" style="margin-right: 8px; color: #94a3b8;" aria-hidden="true"></i>Now Showing (Top 5 Movies)</h3>
        <asp:GridView ID="gvNowShowing" runat="server" CssClass="table" AllowPaging="false" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="Title" HeaderText="Title" />
                <asp:BoundField DataField="Genre" HeaderText="Genre" />
                <asp:BoundField DataField="Language" HeaderText="Language" />
                <asp:BoundField DataField="Duration" HeaderText="Duration (min)" />
                <asp:BoundField DataField="ReleaseDate" HeaderText="Release Date" DataFormatString="{0:yyyy-MM-dd}" />
            </Columns>
            <EmptyDataTemplate>
                <tr><td colspan="5" style="text-align: center; padding: 20px; color: #94a3b8;">No movies found</td></tr>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <!-- RECENT BOOKINGS -->
    <div class="data-section">
        <h3><i class="fa-solid fa-calendar-check" style="margin-right: 8px; color: #94a3b8;" aria-hidden="true"></i>Recent Bookings</h3>
        <asp:GridView ID="gvRecentBookings" runat="server" CssClass="table" AllowPaging="false" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="ConfirmationCode" HeaderText="Confirmation #" />
                <asp:BoundField DataField="UserName" HeaderText="Customer" />
                <asp:BoundField DataField="ShowDate" HeaderText="Show Date" DataFormatString="{0:yyyy-MM-dd}" />
            </Columns>
            <EmptyDataTemplate>
                <tr><td colspan="3" style="text-align: center; padding: 20px; color: #94a3b8;">No bookings found</td></tr>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <!-- HALLS OVERVIEW -->
    <div class="data-section">
        <h3><i class="fa-solid fa-door-open" style="margin-right: 8px; color: #94a3b8;" aria-hidden="true"></i>Halls Overview</h3>
        <asp:GridView ID="gvHalls" runat="server" CssClass="table" AllowPaging="false" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="HallName" HeaderText="Hall Name" />
                <asp:BoundField DataField="HallCapacity" HeaderText="Capacity" />
                <asp:BoundField DataField="HallType" HeaderText="Type" />
                <asp:BoundField DataField="TheatreName" HeaderText="Theatre" />
            </Columns>
            <EmptyDataTemplate>
                <tr><td colspan="4" style="text-align: center; padding: 20px; color: #94a3b8;">No halls found</td></tr>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <!-- UPCOMING SHOWS -->
    <div class="data-section">
        <h3><i class="fa-solid fa-clock" style="margin-right: 8px; color: #94a3b8;" aria-hidden="true"></i>Upcoming Shows</h3>
        <asp:GridView ID="gvUpcomingShows" runat="server" CssClass="table" AllowPaging="true" PageSize="10" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="Title" HeaderText="Movie" />
                <asp:BoundField DataField="ShowDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="ShowTime" HeaderText="Time" />
                <asp:BoundField DataField="HallName" HeaderText="Hall" />
            </Columns>
            <PagerStyle BackColor="#1e293b" ForeColor="#f1f5f9" />
            <EmptyDataTemplate>
                <tr><td colspan="4" style="text-align: center; padding: 20px; color: #94a3b8;">No shows scheduled</td></tr>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <script type="text/javascript">
        function renderPaymentChart() {
            var paymentData = document.getElementById('<%= hdnPaymentData.ClientID %>').value;
            if (!paymentData) return;

            var data = JSON.parse(paymentData);
            var ctx = document.getElementById('paymentStatusChart').getContext('2d');

            new Chart(ctx, {
                type: 'pie',
                data: {
                    labels: data.labels,
                    datasets: [{
                        data: data.values,
                        backgroundColor: [
                            '#16a34a',
                            '#eab308',
                            '#6B4423',
                            '#64748b'
                        ],
                        borderColor: '#1e293b',
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                color: '#e2e8f0',
                                font: { size: 12 },
                                padding: 15
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    var label = context.label || '';
                                    var value = context.parsed || 0;
                                    var total = context.dataset.data.reduce((a, b) => a + b, 0);
                                    var percentage = ((value / total) * 100).toFixed(1);
                                    return label + ': ' + value + ' (' + percentage + '%)';
                                }
                            }
                        }
                    }
                }
            });
        }

        function renderTicketChart() {
            var ticketData = document.getElementById('<%= hdnTicketData.ClientID %>').value;
            if (!ticketData) return;

            var data = JSON.parse(ticketData);
            var ctx = document.getElementById('ticketStatusChart').getContext('2d');

            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: data.labels,
                    datasets: [{
                        data: data.values,
                        backgroundColor: [
                            '#2563eb',
                            '#16a34a',
                            '#6B4423',
                            '#64748b'
                        ],
                        borderColor: '#1e293b',
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                color: '#e2e8f0',
                                font: { size: 12 },
                                padding: 15
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    var label = context.label || '';
                                    var value = context.parsed || 0;
                                    var total = context.dataset.data.reduce((a, b) => a + b, 0);
                                    var percentage = ((value / total) * 100).toFixed(1);
                                    return label + ': ' + value + ' (' + percentage + '%)';
                                }
                            }
                        }
                    }
                }
            });
        }

        window.addEventListener('load', function () {
            renderPaymentChart();
            renderTicketChart();
        });
    </script>
</asp:Content>
