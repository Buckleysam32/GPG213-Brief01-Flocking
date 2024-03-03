using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BoidManager;

public class Boid : MonoBehaviour
{
    // Basic properties of a Boid, used for Euler Integration
    public Vector2 vel;
    public Vector2 pos;
    public Vector2 force;

    // Accumulate force
    // Every update the BoidManager uses the Boid's force then wipes it to zero
    public void AddForce(Vector2 f)
    {
        force += f;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Store the Boid's transform position into it's pos
        pos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Find all nearby Boids
        var nearby = BoidManager.instance.FindBoidsInRange(this, pos, BoidManager.instance.boidSightRange);
        // If there are nearby Boids
        if (nearby.Count > 0)
        {
            if (BoidManager.instance.boidEnableCohesion)
            {
                // Do Cohesion processing here
                Vector2 cohesionForce = Cohesion(nearby);
                AddForce(cohesionForce);
            }

            if (BoidManager.instance.boidEnableSeparation)
            {
                //Do Seperation Logic
                Vector2 sepForce = Seperation(nearby);
                AddForce(sepForce);
            }

            if (BoidManager.instance.boidEnableAlignment)
            {
                //Do Alignment Logic
            }
        }

        if (BoidManager.instance.edge == EdgeBehavior.Collide)
        {
            AvoidEdge();
        }
    }
    private float CalculateAvoidanceForce(float distanceFromNearEdge, float distanceFromFarEdge)
    {
        float avoidanceForce = 0.0f;
        float avoidanceRadius = 10.0f; // Adjust this value as needed

        // Check if boid is close to the near edge
        if (distanceFromNearEdge < avoidanceRadius)
        {
            // Calculate avoidance force proportional to the distance from the near edge
            avoidanceForce += Mathf.Clamp01(1.0f - (distanceFromNearEdge / avoidanceRadius));
        }

        // Check if boid is close to the far edge
        if (distanceFromFarEdge < avoidanceRadius)
        {
            // Calculate avoidance force proportional to the distance from the far edge
            avoidanceForce -= Mathf.Clamp01(1.0f - (distanceFromFarEdge / avoidanceRadius));
        }

        return avoidanceForce;
    }

    private void AvoidEdge()
    {
        Vector2 avoidanceForce = Vector2.zero;

        // Calculate distance from each edge
        float distanceToLeftEdge = pos.x;
        float distanceToRightEdge = BoidManager.instance.worldSize.x - pos.x;
        float distanceToBottomEdge = pos.y;
        float distanceToTopEdge = BoidManager.instance.worldSize.y - pos.y;

        // Calculate avoidance force based on distance to each edge
        avoidanceForce.x = CalculateAvoidanceForce(distanceToLeftEdge, distanceToRightEdge);
        avoidanceForce.y = CalculateAvoidanceForce(distanceToBottomEdge, distanceToTopEdge);

        // Apply avoidance force
        force += avoidanceForce.normalized * BoidManager.instance.boidSpeed * BoidManager.instance.boidStrengthAvoidance;
    }

    private Vector2 Cohesion(List<Boid> nearbyBoids)
    {
        Vector2 avgPos = Vector2.zero;

        foreach (var boid in nearbyBoids)
        {
            avgPos += boid.pos;
        }

        avgPos /= nearbyBoids.Count;

        Vector2 newDir = (avgPos - pos).normalized;

        return newDir * BoidManager.instance.boidStrengthCohesion;
    }

    private Vector2 Seperation(List<Boid> nearbyBoids)
    {
        Vector2 sepForce = Vector2.zero;

        foreach(var boid in nearbyBoids)
        {
            Vector2 spaceBetween = pos - boid.pos;

            float distance = spaceBetween.magnitude;
            if(distance > 0)
            {
                spaceBetween /= distance * distance;
            }

            sepForce += spaceBetween;
        }

        if(nearbyBoids.Count > 0)
        {
            sepForce /= nearbyBoids.Count;
        }

        return sepForce * BoidManager.instance.boidStrengthSeparation;
    }
}
