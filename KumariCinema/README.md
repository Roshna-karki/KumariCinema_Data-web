# ?? KUMARI CINEMA MANAGEMENT SYSTEM

## Quick Start Guide

### Prerequisites
- Visual Studio 2019/2022
- .NET Framework 4.7.2
- Oracle Database 21c XE or higher
- Oracle.ManagedDataAccess NuGet Package

---

## ?? Installation Steps

### Step 1: Database Setup
```sql
-- Connect as SYSTEM
sqlplus system/password@localhost:1522/XEPDB1

-- Create user
CREATE USER cinema IDENTIFIED BY 1234;
GRANT CONNECT, RESOURCE TO cinema;
GRANT UNLIMITED TABLESPACE TO cinema;

-- Follow all SQL scripts in SETUP_GUIDE.txt (Step 2-5)
```

### Step 2: Install NuGet Packages
```
Package Manager Console:
Install-Package Oracle.ManagedDataAccess
```

### Step 3: Update Connection String
File: `Web.config`
```xml
<connectionStrings>
    <add name="OracleConnectionString" 
         connectionString="User Id=cinema;Password=1234;Data Source=localhost:1522/XEPDB1;" 
         providerName="Oracle.ManagedDataAccess.Client"/>
</connectionStrings>
```

### Step 4: Run Application
- Visual Studio > Debug > Start Debugging (F5)
- Navigate to: `http://localhost:port/Dashboard.aspx`

---

## ?? Application Pages

| Page | URL | Function |
|------|-----|----------|
| Dashboard | `/Dashboard.aspx` | Statistics & Charts |
| Movies | `/MovieDetails.aspx` | Movie CRUD |
| Theatres | `/TheatreDetails.aspx` | Theatre CRUD |
| Halls | `/HallDetails.aspx` | Hall CRUD |
| Showtimes | `/ShowtimeDetails.aspx` | Show Scheduling |
| Bookings | `/BookingDetails.aspx` | Booking Management |
| Tickets | `/TicketDetails.aspx` | Ticket Issuance |
| Payments | `/PaymentDetails.aspx` | Payment Recording |
| Seats | `/SeatDetails.aspx` | Seat Management |
| Customers | `/CustomerDetails.aspx` | Customer Registration |

---

## ?? Features

? Dark Modern UI  
? Responsive Design  
? Real-time Dashboard Statistics  
? Interactive Charts (Chart.js)  
? Complete CRUD Operations  
? Search & Filter  
? Pagination (20 items per page)  
? Form Validation  
? Error Handling  
? Oracle Database Integration  

---

## ?? Technical Stack

- **Frontend**: ASP.NET Web Forms, Bootstrap, Chart.js
- **Backend**: C#, .NET Framework 4.7.2
- **Database**: Oracle Database 21c XE
- **Authentication**: Forms-based (can be extended)
- **ORM**: ADO.NET with Parameterized Queries

---

## ?? Database Tables

1. **Users** - Customer information
2. **Theatre** - Cinema locations
3. **Hall** - Theatre halls/screens
4. **Movie** - Movie catalog
5. **Shows** - Show timings
6. **Booking** - Customer bookings
7. **Payment** - Payment records
8. **Ticket** - Issued tickets
9. **Seat** - Seating information
10. **Price** - Pricing configuration
11. **Cancellation** - Cancellation records

---

## ?? Default Login

**Username**: admin  
**Password**: admin  

(Note: Implement your own authentication system in production)

---

## ?? Common Issues

### Error: "Oracle.ManagedDataAccess not found"
```
Solution: Install NuGet Package
Install-Package Oracle.ManagedDataAccess
```

### Error: "Cannot connect to database"
```
Solution: Check connection string in Web.config
User Id=cinema;Password=1234;Data Source=localhost:1522/XEPDB1;
```

### Error: "Table does not exist"
```
Solution: Run all CREATE TABLE statements from SETUP_GUIDE.txt
Connect as CINEMA user and verify with: SELECT * FROM user_tables;
```

---

## ?? Sample Queries

### Get Total Movies
```sql
SELECT COUNT(*) FROM Movie;
```

### Get Today's Revenue
```sql
SELECT NVL(SUM(PaymentAmount), 0) FROM Payment 
WHERE UPPER(PaymentStatus) = 'COMPLETED' 
AND TRUNC(PaymentDate) = TRUNC(SYSDATE);
```

### Get Active Shows
```sql
SELECT * FROM Shows 
WHERE ShowDate >= TRUNC(SYSDATE) 
ORDER BY ShowDate, ShowTime;
```

---

## ?? Security Notes

? Parameterized Queries (SQL Injection Protection)  
? Try-Catch Error Handling  
? Input Validation  
?? TODO: Implement user authentication  
?? TODO: Add HTTPS/SSL encryption  
?? TODO: Implement role-based access control  

---

## ?? Support

For issues or questions:
1. Check the SETUP_GUIDE.txt file
2. Review error messages in the application
3. Check Debug Output window (Visual Studio)
4. Verify database connectivity with SQL*Plus

---

## ?? License

Kumari Cinema Management System - Private Project

---

**Version**: 1.0  
**Last Updated**: 2024  
**Framework**: .NET Framework 4.7.2  
**Database**: Oracle Database 21c XE
