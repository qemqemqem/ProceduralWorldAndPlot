
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Object to indicate where cameras are, so the map streamer knows what to load
/// </summary>
public interface IMapFocus{
	Vector3 getCenter();//based on the camera focus
	Vector3 getExtents();//based on the camera distance from the ground plane
}


/// <summary>
/// Streams and generates map as the cameras move around the world
/// To start lets just procedurally generate everything
/// Later on let's store deltas on disk and load from that
/// To start lets just do simple rectangular tiles with blocks on them
/// Later on let's store meshes that stitch
/// To start let's just assume one LOD and things farther away just don't exist
/// Later on let's make it so we can zoom out and support different camera modes on different meshes (surface hugging with various perspective locks vs orbiting)
/// </summary>
public class MapStreamer2D{
	private HashSet<NavMeshSurface> surfacesToUpdate = new HashSet<NavMeshSurface> ();
	private HashSet<NavMeshSurface> surfaces = new HashSet<NavMeshSurface> ();
	private HashSet<IMapFocus> focalPoints = new HashSet<IMapFocus> ();


	private int regionSize=10;

	
	// Update is called once per frame
	void Update () {
		//check if a focus is near the edge of a region that is not loaded
		//if the focus is near the edge of a region that is not loaded load the adjacent regions

		foreach (NavMeshSurface surface in surfacesToUpdate) {
			surface.BuildNavMesh ();
		}


		
	}

	public void GenerateRegion(int x, int y){
		for (int i = 0; i < regionSize; ++i) {
			for (int j = 0; j < regionSize; ++j) {
				//spawn prefab tiles around the x,y center
			}
		}
	}

	public void GenerateMap(int x, int y){
	}

	public void AddFocus(IMapFocus focus){
		if(!focalPoints.Contains(focus))
			focalPoints.Add (focus);
	}

	public void RemoveFocus(IMapFocus focus){
		if(focalPoints.Contains(focus))
			focalPoints.Remove (focus);
	}


	public List<Vector3> getTilesToLoad(){
		foreach (var focus in focalPoints) {
			
		}
	}
}
*/