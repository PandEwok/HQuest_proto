using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    GridManager gridManager;

    [HideInInspector] public Vector2 positionInGrid = new Vector2(0,0);

    List<Tile> highlightedTiles = new();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridManager = transform.parent.GetComponent<GameManager>().gridManager;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*public void Move(Vector2 newPos)
    {
        Tile tile;
        gridManager._tiles.TryGetValue(newPos, out tile);
        Vector3 worldPos = tile.surface.transform.position;

        positionInGrid = newPos;
        transform.position = worldPos;
    }*/
    public IEnumerator MoveAlongPath(List<Tile> path)
    {
        foreach (Tile tile in path)
        {
            Vector3 targetPos = tile.surface.transform.position;

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        targetPos,
                        4f * Time.deltaTime);

                yield return null;
            }

            transform.position = targetPos;

            positionInGrid = gridManager.GetTileCoords(tile);

            yield return new WaitForSeconds(0.05f);
        }
    }


    public List<Tile> FindPath(Tile start, Tile goal)
    {
        Queue<Tile> frontier = new();
        Dictionary<Tile, Tile> cameFrom = new();

        frontier.Enqueue(start);
        cameFrom[start] = null;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (Tile next in gridManager.GetNeighbours(current))
            {
                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return null;

        List<Tile> path = new();

        Tile currentTile = goal;

        while (currentTile != start)
        {
            path.Add(currentTile);
            currentTile = cameFrom[currentTile];
        }

        path.Reverse();

        return path;
    }

    public List<Tile> GetReachableTiles(Tile start, int maxMovement)
    {
        List<Tile> reachable = new();

        Queue<(Tile, int)> frontier = new();

        HashSet<Tile> visited = new();

        frontier.Enqueue((start, 0));

        visited.Add(start);

        while (frontier.Count > 0)
        {
            var (current, distance) = frontier.Dequeue();

            reachable.Add(current);

            if (distance >= maxMovement)
                continue;

            foreach (Tile neighbour in gridManager.GetNeighbours(current))
            {
                if (visited.Contains(neighbour))
                    continue;

                visited.Add(neighbour);

                frontier.Enqueue((neighbour, distance + 1));
            }
        }

        return reachable;
    }

    public void ShowMovementRange(int movement)
    {
        ClearMovementRange();

        Tile currentTile =
            gridManager.GetTileAtPosition(positionInGrid);

        List<Tile> reachable =
            GetReachableTiles(currentTile, movement);

        foreach (Tile tile in reachable)
        {
            tile.ChangeIndicator(new Color(0.5f, 0.5f, 1, 0.8f));
            tile.actionIndicator.SetActive(true);

            highlightedTiles.Add(tile);
        }
    }
    public void ClearMovementRange()
    {
        foreach (Tile tile in highlightedTiles)
        {
            if (tile != null)
                tile.actionIndicator.SetActive(false);
        }

        highlightedTiles.Clear();
    }
}
