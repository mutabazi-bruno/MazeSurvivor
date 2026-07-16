// plain data holder - one of these exists per grid cell
// every cell starts fully walled in on all 4 sides, the algorithm knocks walls down as it carves paths
public class MazeCell
{
    public bool visited = false;
    public bool hasTopWall = true;
    public bool hasRightWall = true;
    public bool hasBottomWall = true;
    public bool hasLeftWall = true;
}