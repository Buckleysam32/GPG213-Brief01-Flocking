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

    public Collider2D squareCollider;

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

        squareCollider = GameObject.Find("Square").GetComponent<Collider2D>();
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
                Vector2 alignForce = Alignment(nearby);
                AddForce(alignForce);
            }
        }

        if (BoidManager.instance.edge == EdgeBehavior.Collide)
        {
            AvoidEdge();
        }

        AvoidCollider();
    }



    private float CalculateAvoidanceForce(float distanceFromNearEdge, float distanceFromFarEdge)
    {
        float avoidanceForce = 0.0f;
        float avoidanceRadius = 10.0f; 

        
        if (distanceFromNearEdge < avoidanceRadius)
        {
            avoidanceForce += Mathf.Clamp01(1.0f - (distanceFromNearEdge / avoidanceRadius));
        }

        if (distanceFromFarEdge < avoidanceRadius)
        {
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

    private void AvoidCollider()
    {
        Vector2 avoidForce = Vector2.zero;

        if (squareCollider != null)
        {
            Vector2 closestPoint = squareCollider.ClosestPoint(pos);
            float distance = Vector2.Distance(pos, closestPoint);
            float avoidanceRadius = 5.0f; 

            
            if (distance < avoidanceRadius)
            {
                
                avoidForce = (pos - closestPoint).normalized;
            }
        }

        force += avoidForce * BoidManager.instance.boidStrengthAvoidance;
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

    private Vector2 Alignment(List<Boid> nearbyBoids)
    {
        Vector2 avgVel = Vector2.zero;

        foreach(var boid in nearbyBoids)
        {
            avgVel += boid.vel;
        }

        avgVel /= nearbyBoids.Count;

        Vector2 newDir = avgVel.normalized;

        Vector2 alignForce = (newDir - vel.normalized) * BoidManager.instance.boidStrengthAlignment;

        return alignForce;
    }
}
