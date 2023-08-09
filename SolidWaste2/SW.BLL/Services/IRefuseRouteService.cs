namespace SW.BLL.Services;

public interface IRefuseRouteService
{
    Task<RouteResponse> SearchRefuseRoute(string address, CancellationToken cancellationToken = default);
}

public class RouteResponse
{
    public SpatialReference SpatialReference { get; set; }
    public List<RouteCandidate> Candidates { get; set; }
}

public class SpatialReference
{
    public int Wkid { get; set; }
    public int LatestWkid { get; set; }
}

public class RouteCandidate
{
    public string Address { get; set; }
    public RouteLocation Location { get; set; }
    public double Score { get; set; }
    public RouteAttributes Attributes { get; set; }
}

public class RouteLocation
{
    public double X { get; set; }
    public double Y { get; set; }
    public double M { get; set; }
}

public class RouteAttributes
{
    //public string User_fld { get; set; }
    public string Day_Route { get; set; }

    private bool parsed = false;
    private string day;
    private string route;
    private string color;
    private string recycling_route;

    private void Parse()
    {
        if (parsed)
            return;
        //var strings = User_fld.ToLower().Split(',');
        var strings = Day_Route.ToLower().Split(',');
        day = strings[0];
        route = strings[1];
        color = strings[2];
        recycling_route = strings[3];
        parsed = true;
    }

    public string Route
    {
        get
        {
            Parse();
            return route;
        }
    }

    public bool IsMonday
    {
        get
        {
            Parse();
            return day == "monday";
        }
    }

    public bool IsTuesday
    {
        get
        {
            Parse();
            return day == "tuesday";
        }
    }

    public bool IsWednesday
    {
        get
        {
            Parse();
            return day == "wednesday";
        }
    }

    public bool IsThursday
    {
        get
        {
            Parse();
            return day == "thursday";
        }
    }

    public bool IsFriday
    {
        get
        {
            Parse();
            return day == "friday";
        }
    }

    public bool IsSaturday
    {
        get
        {
            Parse();
            return day == "saturday";
        }
    }

    public bool IsSunday
    {
        get
        {
            Parse();
            return day == "sunday";
        }
    }

    public bool IsRed
    {
        get
        {
            Parse();
            return color == "red";
        }
    }

    public bool IsBlue
    {
        get
        {
            Parse();
            return color == "blue";
        }
    }
}
