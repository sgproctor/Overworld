using UnityEngine;
using Unity.Mathematics;

public interface IheightIncrease
{
    public float heightAt(Vector2 point);
}

public class HeightBlob : IheightIncrease
{
    public Vector2 midPoint{get;set;}
    public float radius{get;set;}

    public float heightAt(Vector2 point)
    {
        float distance = Mathf.Clamp(Vector2.Distance(point, midPoint) , 0, radius);
        return (1f - Mathf.InverseLerp(0, radius, distance));
    }

    public HeightBlob(float radius, float width, float height)
    {
        this.radius = radius;
        this.midPoint = new Vector2(UnityEngine.Random.Range(0f, width-radius), 
                                    UnityEngine.Random.Range(0f, height-radius));
    }

    public HeightBlob(float radius, Vector2 point)
    {
        this.radius = radius;
        this.midPoint = point;
    }
}


public class HeightNoise : IheightIncrease
{
    public Vector2 Origin;
    public float noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;

    public float heightAt(Vector2 point)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++) {
            float xVal = point.x / noiseScale * frequency + Origin.x;
            float yVal = point.y / noiseScale * frequency + Origin.y;

            float perlinValue = noise.snoise(new float2(xVal, yVal));
            perlinValue *= perlinValue;
            noiseHeight += Mathf.InverseLerp(-1f, 2f, perlinValue) * amplitude;
            //noiseHeight *= noiseHeight;

            amplitude *= persistance;
            frequency *= lacunarity;
        }
        return noiseHeight;
    }

    public HeightNoise(Vector2 origin, float scale, int octaves, float persistance, float lacunarity)
    {
        this.Origin = origin;
        this.noiseScale = scale;
        this.octaves = octaves;
        this.persistance = persistance;
        this.lacunarity = lacunarity;
    }
}


public class PerturbedCircle : IheightIncrease
{
    public Vector2 midPoint{get;set;}
    public float radius{get;set;}
    public Vector2 Origin;
    public float noiseScale;
    public float perturbScale;
    
    private float perturbNoise(Vector2 point)
    {
        float xVal = ((point.x) / (noiseScale)) + Origin.x;
        float yVal = ((point.y) / (noiseScale)) - Origin.y;
        return perturbScale * noise.snoise(new float2(xVal, yVal));
    }

    public float heightAt(Vector2 point)
    {
        float xVal = ((point.x) / (noiseScale)) + Origin.x;
        float yVal = ((point.y) / (noiseScale)) - Origin.y;
        Vector2 perturbPoint = new Vector2(point.x + perturbScale * noise.snoise(new float2(xVal, yVal)),
                                           point.y + perturbScale * noise.snoise(new float2(xVal, yVal)));
        float distance = Mathf.Clamp(Vector2.Distance(perturbPoint, midPoint) , 0, radius);
        //return(radius > distance ? 1f: 0f);
        float height = (1f - Mathf.Pow(Mathf.InverseLerp(0, radius, distance),2f));
        //if(height >= 0.2f) return Mathf.Lerp(0.2f, 1f, height);
        return height;
    }

    public PerturbedCircle(float radius, float width, float height, float scale, Vector2 origin,
        float perturbScale)
    {
        this.radius = radius;
        this.midPoint = new Vector2(UnityEngine.Random.Range(radius+perturbScale, width-radius-perturbScale), 
                                    UnityEngine.Random.Range(radius+perturbScale, height-radius-perturbScale));
        this.noiseScale = scale;
        this.Origin = origin;
        this.perturbScale = perturbScale;
    }

    public PerturbedCircle(float radius, Vector2 point, float scale, Vector2 origin, float perturbScale)
    {
        this.radius = radius;
        this.midPoint = point;
        this.noiseScale = scale;
        this.Origin = origin;
        this.perturbScale = perturbScale;
    }
}


public class RandomSlope : IheightIncrease
{
    private Vector2 startingPoint;
    private Vector2 oppositePoint;
    private float slope;
    private float perpendicularSlope;
    private Vector2 direction;

    public RandomSlope(float width, float height)
    {
        float startX = UnityEngine.Random.Range(0, width);
        float startY = UnityEngine.Random.Range(0, height);
        float minAxis = Mathf.Min(startX, startY);
        startX = startX - minAxis;
        startY = startY - minAxis;
        // float startY = 0f;
        
        float oppositeX = width - startX;
        // float oppositeY = height;
        float oppositeY = height - startY;
        
        startingPoint = new Vector2(startX, startY);
        oppositePoint = new Vector2(oppositeX, oppositeY);
        slope = (oppositeY-startY)/(oppositeX-startX);
        perpendicularSlope = -1f / slope;
        direction = new Vector2(0,100);
    }

    public Vector2 getIntersection(Vector2 point)
    {
        float x = (startingPoint.x * slope - point.x * perpendicularSlope + point.y - startingPoint.y) / (slope - perpendicularSlope); 
        float y = slope * (x - startingPoint.x) + startingPoint.y;
        return new Vector2(x,y);
    }

    public float heightAt(Vector2 point)
    {
        float endToEndDistance = Vector2.Distance(startingPoint, oppositePoint);
        float endToPointDistance = Vector2.Distance(startingPoint, getIntersection(point));
        //return 1f;
        return (endToPointDistance / endToEndDistance);
    }

}
