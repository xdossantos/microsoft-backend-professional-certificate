# AcmeGrocer Customer Prioritization System

A data-driven solution for car dealership customer prioritization using linear programming and ASP.NET Core Web API.

## 🚀 Quick Start

```bash
# Restore dependencies
dotnet restore AcmeGrocer.csproj

# Build the project
dotnet build AcmeGrocer.csproj

# Run the application
dotnet run --project AcmeGrocer.csproj
```

The application will be available at: `http://localhost:5096`
- Swagger UI for API testing
- 1000 sample customers automatically loaded
- Customer prioritization algorithm ready to use

## 📋 Key Features

- **Smart Customer Scoring**: Linear programming algorithm with weighted criteria
- **RESTful API**: Complete CRUD operations with customer recommendations
- **Binary Serialization**: Efficient data storage and export
- **Data Analysis**: Comprehensive Jupyter notebook with insights
- **Production Ready**: Dependency injection, logging, and error handling

## 📊 API Usage

### Get Customer Recommendations
```http
POST /api/customers/recommendations
Content-Type: application/json

{
  "latitude": 40.7128,
  "longitude": -74.0060,
  "count": 10
}
```

### Get Customer Statistics
```http
GET /api/customers/statistics
```

## 📓 Documentation

- **Complete Implementation Guide**: See `README-SOLUTION.md`
- **Data Analysis Notebook**: See `CustomerAnalysis.ipynb`
- **API Testing**: Use `customer-api-tests.http`

---

Built with ASP.NET Core, Entity Framework, and modern C# practices.
