using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WeatherManager : MonoBehaviour
{
    public Grid<Vector2> initialWindGrid;
    public Grid<float> heightGrid;
    public Grid<float> precipitationGrid;    
    // public MeshRenderer meshRenderer;
    // public MeshFilter meshFilter;
    // public Material meshMaterial;
    int height;
    int width;
    int cellSize;
    float waterLevel;
    float articLine;
    float tropicLine;
    float equator;
    [SerializeField] int windIterations;
    [SerializeField] int initialWaterVal;
    [SerializeField] float slopeDelfectionMod = 4;
    public Gradient precipGrad;
    [SerializeField] float maxPrecip;
    [SerializeField] int maxFilterIterations;
    [SerializeField] int precipitationFilterWidth;
    [SerializeField] bool drawWind;
    List<float> precipMap;

    delegate void LoopDelegate(int x, int y);

    public void RunSim(int height, int width, int cellSize, float waterLevel, float[,] heightMap)
    {
        precipMap = new List<float>();
        this.height = height;
        this.width = width;
        this.cellSize = cellSize;
        this.waterLevel = waterLevel;

        SetLatitudes(artic: height * 0.8f, tropic: height * 0.40f, equator: height * 0.20f);

        heightGrid = new Grid<float>(width, height, cellSize);
        initialWindGrid = new Grid<Vector2>(width, height, cellSize);
        precipitationGrid = new Grid<float>(width, height, cellSize);

        InitializeWindVectors();

        InitializeHeightMap(heightMap);
        CalculateHeightDelfection();
        InitializePrecipitationMap(SetWeatherSeedPoints());
        maxPrecip = GetMaxValueInGrid(precipitationGrid);
        PrecipFilter();
        SetPrecipMapToQuantiles();

        // ContructMesh();
    }

    float GetMaxValueInGrid(Grid<float> grid)
    {
        float maxVal = 0;
        for(int x = 0; x < width/cellSize; x++)
        {
            for(int y = 0; y < height/cellSize; y++)
            {
                maxVal = Mathf.Max(maxVal, grid.getItem(x,y));
            }
        }
        return maxVal;
    }

    // float[,] CreateHeightMap(VoronoiGenerator vGen)
    // {
    //     float[,] heightmap = new float[width/cellSize,height/cellSize];
    //     for(int x = 0; x < width/cellSize; x ++)
    //     {
    //         for(int y = 0; y < height/cellSize; y++)
    //         {
    //             //float temp = GetCellAtPoint(new Vector2(x,y)).height;
    //             heightmap[x,y] = vGen.getHeightAtPoint(new Vector2(x*cellSize,y*cellSize));
    //             if(heightmap[x,y] < waterLevel) waterCellCount ++;
    //         }
    //     }
    //     return heightmap;
    // }

    private void getWaterValue(int x, int y, ref float amount, ref int neighborCount){
        if(heightGrid.getItem(x,y) < waterLevel) return;
		amount += precipitationGrid.getItem(x,y);
		neighborCount += 1;
	}

    private void PrecipFilter()
    {
		// for(int y = 0; y < height/cellSize; y ++){
		// 	for(int x = 0; x < width/cellSize; x ++){
		// 		if(heightGrid.getItem(x,y) < waterLevel){
		// 			precipitationGrid.insertItem(x,y,maxPrecip);
		// 		}
		// 	}
		// }

        maxPrecip = 0;
		for(int iterations = 0; iterations < maxFilterIterations; iterations ++){
			for(int x = 0; x < width/cellSize; x ++){
				for(int y = 0; y < height/cellSize; y ++){

					if((y - precipitationFilterWidth) < 0 || (y + precipitationFilterWidth) > height/cellSize ||
						(x - precipitationFilterWidth) < 0 || (x + precipitationFilterWidth) > width/cellSize){
							continue;
						}
					if(heightGrid.getItem(x,y) <= waterLevel * 0f) continue;
								
					float neighborWatersum = 0f;
					int numNeighbors = 0;
					for(int j = 0; j <= precipitationFilterWidth; j++){
						for(int k = 0; k <= precipitationFilterWidth; k++){
							if(k == 0 && j == 0){
								getWaterValue(x, y, ref neighborWatersum, ref numNeighbors);
							}
							else if(k == 0){
								getWaterValue(x-j,y, ref neighborWatersum, ref numNeighbors);
								getWaterValue(x+j,y, ref neighborWatersum, ref numNeighbors);
							}
							else if(j == 0){
								getWaterValue(x,y+k, ref neighborWatersum, ref numNeighbors);
								getWaterValue(x,y-k, ref neighborWatersum, ref numNeighbors);
							}
							else{
								getWaterValue(x - j,y + k, ref neighborWatersum, ref numNeighbors);
								getWaterValue(x - j,y - k, ref neighborWatersum, ref numNeighbors);
								getWaterValue(x + j,y + k, ref neighborWatersum, ref numNeighbors);
								getWaterValue(x + j,y - k, ref neighborWatersum, ref numNeighbors);
							}
						}
					}
                    precipitationGrid.insertItem(x,y, neighborWatersum / numNeighbors);
                    precipMap.Add(neighborWatersum / numNeighbors);
					//map[x,y].precipitationLevel = neighborWatersum / numNeighbors;
					if(neighborWatersum / numNeighbors > maxPrecip){
						maxPrecip = neighborWatersum / numNeighbors;
					}
				}
			}
        }
        Debug.Log("sorting");
        precipMap.Sort();
    }

    void OnDrawGizmosSelected() 
    { 
        if(height == 0 || width == 0) return;
        void func(int x, int y) 
        {
            //float precip = precipitationGrid.getItem(x,y);
            float precip = heightGrid.getItem(x,y);
            //if(heightGrid.getItem(x,y) < waterLevel) return;
            Gizmos.color = precipGrad.Evaluate(precip);
            Gizmos.DrawCube(new Vector2(x*cellSize, y*cellSize), new Vector2(cellSize, cellSize));
        }
        LoopDelegate test = func;
        GridLoop(test);
        // Draw a semitransparent blue cube at the transforms position
    }

    void SetPrecipMapToQuantiles()
    {
        void func(int x, int y) 
        {
            float precip = precipitationGrid.getItem(x,y);
            precip = (precipMap.IndexOf(precip)) / (float)(precipMap.Count());
            precipitationGrid.insertItem(x,y,precip);
        }
        LoopDelegate test = func;
        GridLoop(test);
    }

    public void SetGridSize(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public float[,] SetWeatherSeedPoints()
    {
        float[,] initialWaterMap = new float[width/cellSize,height/cellSize];
        for (int i = 0; i < windIterations; i++) {
            int randomX = UnityEngine.Random.Range(0, width/cellSize);
            int randomY = UnityEngine.Random.Range(0, height/cellSize);
            //initialWaterMap[randomX,randomY] = initialWaterVal;
            if(heightGrid.getItem(randomX,randomY) <= waterLevel) initialWaterMap[randomX,randomY] = initialWaterVal;
        }
        return initialWaterMap;
    }

    private void GridLoop(LoopDelegate loopDelegate)
    {
        for(int x = 0; x < width/cellSize - 1; x ++)
        {
            for(int y = 0; y < height/cellSize - 1; y++)
            {
                loopDelegate(x,y);
            }
        } 
    }

    public void SetLatitudes(float artic, float tropic, float equator)
    {
        this.articLine = artic;
        this.tropicLine = tropic;
        this.equator = equator;
    }

    public void InitializeHeightMap(float[,] heightmap)
    {
        void func(int x, int y)
        {
            heightGrid.insertItem(x,y, heightmap[x,y]);
        }
        LoopDelegate test = func;
        GridLoop(test);
    }

    public void InitializeWindVectors()
    {
        
        void func(int x, int y) {initialWindGrid.insertItem(x,y, GetWindDirectionBasedOnLatitude(y * cellSize)*cellSize);}
        LoopDelegate test = func;
        GridLoop(test);
    }

    public void InitializePrecipitationMap(float[,] initialSource)
    {
        void func(int x, int y) 
        {
            float waterVal = initialSource[x,y];
            if(waterVal > 0)
            {
                // loop until all water is removed or out of bounds
                Vector2 globalPosition = new Vector2(x*cellSize, y*cellSize);
                Vector2 direction = Vector2.zero;
                float height = 0;
                float waterAtGrid = 0;
                int count = 0;
                while(!PointOutOfBounds(globalPosition) && waterVal > 0)
                {
                    if(count >= 200) break;
                    count ++;
                    // dump water or gain water depending on heights
                    height = heightGrid.getItem(globalPosition);
                    if(height >= waterLevel)
                    //if(height > 0)
                    {
                        waterAtGrid = precipitationGrid.getItem(globalPosition);
                        waterAtGrid ++;
                        //precipitationGrid.insertItem(x,y, waterAtGrid++);
                        precipitationGrid.insertItem(globalPosition, waterAtGrid);
                        
                        waterVal--;
                    }
                    // else
                    // {
                    //     waterVal++;
                    // }
                    direction = initialWindGrid.getItem(globalPosition);
                    if(drawWind) DrawArrow.ForDebug(globalPosition, direction, Color.red, 100,cellSize / 4f);
                    globalPosition = globalPosition + direction;
                    // get next point on map based on wind direction
                }
            }
        }
        LoopDelegate test = func;
        GridLoop(test);
    }

    private bool PointOutOfBounds(Vector2 point)
    {
        if(point.x > width*cellSize || point.x < 0) return true;
        if(point.y > height*cellSize || point.y < 0) return true;
        else return false;
    }

    public void DrawPrecipitationMap()
    {
        void func(int x, int y) 
        {
            float waterVal = precipitationGrid.getItem(x,y);
            Color waterCol = new Color(0,0,waterVal);
            if(waterVal > 0) DrawArrow.ForDebug(new Vector2((x*cellSize)+cellSize/2f,(y*cellSize)+cellSize/2f), 
                                new Vector2(0, cellSize), waterCol, 100,cellSize / 4f);
        }
        LoopDelegate test = func;
        GridLoop(test);
    }

    public void DrawHeightMap()
    {
        void func(int x, int y) 
        {
            float cellheight = heightGrid.getItem(x,y);
            Color heightCol = new Color(cellheight,cellheight,cellheight);
            if(cellheight > 0) DrawArrow.ForDebug(new Vector2((x*cellSize)+cellSize/2f,(y*cellSize)+cellSize/2f), 
                                new Vector2(0, cellSize * cellheight), heightCol, 100,cellSize / 4f);
        }
        LoopDelegate test = func;
        GridLoop(test);
    }

    public void CalculateHeightDelfection()
    {
        void func(int x, int y)
        {
            if(x <= 0 || y <= 0 || x >= width || y >= height) return;
            
            float currentHeight = heightGrid.getItem(x,y);
            if(currentHeight <= waterLevel) return;
            Vector2 currentWindVect = initialWindGrid.getItem(x,y);

            (int nextX, int nextY) = GetCellOffsetBasedOnVector(currentWindVect);
            float nextHeight = heightGrid.getItem(nextX+x,nextY+y);
            float slope = slopeDelfectionMod*(nextHeight - currentHeight);
            float magnitude = Mathf.Max(cellSize/5f, (cellSize) - (cellSize*slope));

            //y goes from 0 - 1, x goes from 1 to 0 (linearinterpolation of sin and cos)
            float randVal = UnityEngine.Random.Range(-1f,1f);
            Vector2 windDeflection = new Vector2((1-slope)*randVal, randVal*slope);

            initialWindGrid.insertItem(x,y, (slopeDelfectionMod * windDeflection + currentWindVect).normalized*magnitude);
            //DrawArrow.ForDebug(new Vector2((x*cellSize)+cellSize/2f,(y*cellSize)+cellSize/2f), (slopeDelfectionMod * windDeflection + currentWindVect).normalized*magnitude, Color.red, 100,cellSize / 4f);
        }
        LoopDelegate test = func;
        GridLoop(test);
    }

    private (int,int) GetCellOffsetBasedOnVector(Vector2 direction)
    {
        int x, y;
        direction = direction.normalized;
        if(direction.x > 0.5f) x = 1;
        else if(direction.x < -0.5f) x = -1;
        else x = 0;

        if(direction.y > 0.5f) y = 1;
        else if(direction.y < -0.5f) y = -1;
        else y = 0;

        return(x,y);
    }

    public void DrawWindVectors()
    {
        void func(int x, int y) {
            DrawArrow.ForDebug(new Vector2((x*cellSize)+cellSize/2f,(y*cellSize)+cellSize/2f), 
            initialWindGrid.getItem(x,y), Color.gray, 100,cellSize / 4f);}
        LoopDelegate test = func;
        GridLoop(test);
    }

    public Vector2 GetWindDirectionBasedOnLatitude(float y)
    {
        float angle = 0f;
        if (y >= equator)
        {
            //equator
            if(y <= tropicLine){
                angle =  180f + 30f * (y-equator)/(tropicLine - equator);
            }
            //tropic
            else if (y <= articLine){
                //angle = 45 - 90.0f * (y-tropicLine)/(articLine - tropicLine);
                angle = 30f - 30f * (y-tropicLine)/(articLine - tropicLine);
            }
            //artic
            else{
                //angle =  180 + 45f * (y-articLine)/(height - articLine);
                angle = 180 + 30f * (y-articLine)/(height - articLine);
            }
        } 
        else
        {
            //equator to lower tropic
            if(y >= (equator - tropicLine))
            {
                angle =  180f + 30f * (y-equator)/(tropicLine - equator);
            }
            //tropic
            else if (y >= (equator - articLine))
            {
                angle = 30f - 30f * (y-tropicLine)/(articLine - tropicLine);
            }
            //artic
            else{
                angle = 180f + 30f * (y-articLine)/(height - articLine);
            }
        }
        //angle = UnityEngine.Random.Range(0.95f,1.05f)*angle;
        return new Vector2(Mathf.Cos(angle*(Mathf.PI / 180.0f)), Mathf.Sin(angle*(Mathf.PI / 180.0f)));
    }
}

public class Grid<T>
{
    int width;
    int height;
    int cellSize;

    public T[,] gridItems;
    //List<T> gridItems;

    public Grid(int width, int height, int cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.gridItems = new T[width,height];
    }

    public void insertItem(int x, int y, T item)
    {
        gridItems[x*cellSize,y*cellSize] = item;
    }

    public void insertItem(Vector2 point, T item)
    {
        (int x, int y) = getIndexFromPosition(point);
        gridItems[x*cellSize,y*cellSize] = item;
    }

    public T getItem(Vector2 point)
    {
        (int x, int y) = getIndexFromPosition(point);
        return gridItems[x*cellSize,y*cellSize];
    }

    public T getItem(int x, int y)
    {
        return gridItems[x*cellSize,y*cellSize];
    }

    public void drawGrid()
    {
        for(int x = 0; x < width / cellSize; x++)
        {
            for(int y = 0; y < height/cellSize; y++)
            {
                drawCell(new Vector2(x*cellSize,y*cellSize));
            }
        }
    }

    public Vector2 getPositionFromIndex(int x, int y)
    {
        return new Vector2(x*cellSize, y*cellSize);
    }

    public (int x, int y) getIndexFromPosition(Vector2 point)
    {
        return (Mathf.FloorToInt(point.x/cellSize), Mathf.FloorToInt(point.y/cellSize));
    }

    public IEnumerable<T> GetCellNeighbors(int x, int y)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if(i == 0 && j == 0) continue;
                yield return getItem(x+i,y+j);
            }
        }
    }

    public IEnumerable<T> GetCellNeighbors(Vector2 point)
    {
        (int x, int y) = getIndexFromPosition(point);
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if(i == 0 && j == 0) continue;
                yield return getItem(x+i,y+j);
            }
        }
    }

    private void drawCell(Vector2 lowerLeft)
    {
        Vector2 lowerRight = new Vector2(lowerLeft.x + cellSize, lowerLeft.y);
        Vector2 upperLeft = new Vector2(lowerLeft.x, lowerLeft.y + cellSize);
        Vector2 upperRight = new Vector2(lowerRight.x, lowerRight.y + cellSize); 
        Debug.DrawLine(lowerLeft, lowerRight, Color.white, 100);
        Debug.DrawLine(lowerLeft, upperLeft, Color.white, 100);
        Debug.DrawLine(upperLeft, upperRight, Color.white, 100);
        Debug.DrawLine(lowerRight, upperRight, Color.white, 100);
    }
}