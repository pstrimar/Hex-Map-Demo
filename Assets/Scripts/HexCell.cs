using UnityEngine;

public class HexCell : MonoBehaviour
{
    public Color color;
    public HexCoordinates coordinates;
    public RectTransform uiRect;
    public Vector3 Position { get { return transform.localPosition; } }
    public int Elevation 
    {
        get { return elevation; }
        set 
        { 
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position; 

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;
        }
    }
    [SerializeField] HexCell[] neighbors;
    int elevation;

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction) 
    {
		return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
	}

    public HexEdgeType GetEdgeType(HexCell otherCell) 
    {
		return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
	}

}
