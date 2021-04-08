using UnityEngine;

public class Box
{
    public int xMin, xMax, yMin, yMax;

    public Box(int _xMin, int _xMax, int _yMin, int _yMax) {
        xMin = _xMin;
        xMax = _xMax;
        yMin = _yMin;
        yMax = _yMax;
    }

    public Box(int[] coords) {
        xMin = coords[0];
        xMax = coords[1];
        yMin = coords[2];
        yMax = coords[3];
    }

    public bool getOverlap(Box other, out Box overlap) {

        int xMinOverlap = Mathf.Max(xMin, other.xMin);
        int xMaxOverlap = Mathf.Min(xMax, other.xMax);
        int yMinOverlap = Mathf.Max(yMin, other.yMin);
        int yMaxOverlap = Mathf.Min(yMax, other.yMax);

        if (xMinOverlap >= xMaxOverlap || yMinOverlap >= yMaxOverlap) {
            overlap = new Box(0,0,0,0);
            return false;
        }
        overlap = new Box(xMinOverlap, xMaxOverlap, yMinOverlap, yMaxOverlap);
        return true;
    }

    public void displace(int x, int y) {
        xMin += x;
        xMax += x;
        yMin += y;
        yMax += y;
    }

    public int[] getCoords() {
        return new int[] {xMin, xMax, yMin, yMax};
    }

    public int getWidth() {
        return xMax - xMin;
    }

    public int getHeight() {
        return yMax - yMin;
    }

    public override string ToString() {
        return "Box: " + xMin.ToString() + ", " + xMax.ToString() + ", " + yMin.ToString() + ", " + yMax.ToString();
    }
}
