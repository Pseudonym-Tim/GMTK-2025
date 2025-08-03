using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles everything related to the level...
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    public static Dictionary<string, LevelEntityData> LevelEntityDatabase { get; private set; } = null;
    public static Dictionary<string, LevelObjectData> LevelObjectDatabase { get; private set; } = null;
    public List<Entity> LevelEntities { get; private set; } = new List<Entity>();
    public List<LevelObject> LevelObjects { get; private set; } = new List<LevelObject>();

    [SerializeField] private PrefabDatabase prefabDatabase;
    private static EnemyWaveManager enemyWaveManager;

    public void Setup()
    {
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI.FadeIn();

        InitializeLevel();

        // TODO: Ideally, spawn the player after a short cutscene or intro sequence?
        Player playerEntity = (Player)SpawnEntity("player_spaceship", Vector2.zero);

        enemyWaveManager = FindFirstObjectByType<EnemyWaveManager>();
        enemyWaveManager.Setup();
    }

    private void InitializeLevel()
    {
        // Make sure we clear the level first...
        ClearLevel();

        LevelParent = new GameObject("Level");
        LevelEntityParent = new GameObject("Entities");
        LevelObjectParent = new GameObject("Objects");
        LevelParent.transform.SetParent(null, false);
        LevelEntityParent.transform.SetParent(LevelParent.transform);
        LevelObjectParent.transform.SetParent(LevelParent.transform);

        LoadEntityDatabase();
        LoadObjectDatabase();
    }

    private void ClearLevel()
    {
        RemoveEntities(true);
        RemoveObjects();
    }

    private void LoadEntityDatabase()
    {
        LevelEntityDatabase = JsonUtility.LoadJson<string, LevelEntityData>("level_entities");

        foreach(KeyValuePair<string, LevelEntityData> levelEntityData in LevelEntityDatabase)
        {
            levelEntityData.Value.id = levelEntityData.Key;
            levelEntityData.Value.jsonData = (JObject)JsonUtility.ParseJson("level_entities")[levelEntityData.Key];
        }
    }

    private void LoadObjectDatabase()
    {
        LevelObjectDatabase = JsonUtility.LoadJson<string, LevelObjectData>("level_objects");

        foreach(KeyValuePair<string, LevelObjectData> levelObjectData in LevelObjectDatabase)
        {
            levelObjectData.Value.id = levelObjectData.Key;
            levelObjectData.Value.jsonData = (JObject)JsonUtility.ParseJson("level_objects")[levelObjectData.Key];
        }
    }

    public Entity SpawnEntity(string levelEntityType, Vector2 spawnPos, Quaternion spawnRotation = default)
    {
        foreach(string levelEntityID in LevelEntityDatabase.Keys)
        {
            if(levelEntityType == levelEntityID)
            {
                LevelEntityData levelEntityData = LevelEntityDatabase[levelEntityID];
                Entity entityPrefab = prefabDatabase?.GetPrefab<Entity>(levelEntityData.prefab);
                Entity entity = Instantiate(entityPrefab, spawnPos, spawnRotation);
                entity.SetParent(LevelEntityParent.transform, false);
                entity.name = entityPrefab.name;
                entity.LevelEntityData = levelEntityData;
                LevelEntities.Add(entity);
                entity.OnEntitySpawn();
                return entity;
            }
        }

        return null;
    }

    public LevelObject SpawnObject(string levelObjectType, Vector2 spawnPos, Quaternion spawnRotation = default)
    {
        foreach(string levelObjectID in LevelObjectDatabase.Keys)
        {
            if(levelObjectType == levelObjectID)
            {
                LevelObjectData levelObjectData = LevelObjectDatabase[levelObjectID];
                LevelObject objectPrefab = prefabDatabase?.GetPrefab<LevelObject>(levelObjectData.prefab);
                LevelObject levelObject = Instantiate(objectPrefab, spawnPos, spawnRotation);
                levelObject.SetParent(LevelObjectParent.transform, false);
                levelObject.name = objectPrefab.name;
                LevelObjects.Add(levelObject);
                return levelObject;
            }
        }

        return null;
    }

    public void RemoveObjects()
    {
        if(LevelObjects.Count > 0)
        {
            List<LevelObject> objectsToRemoveList = new List<LevelObject>(LevelObjects);

            foreach(LevelObject levelObject in objectsToRemoveList)
            {
                RemoveObject(levelObject.gameObject);
            }
        }
    }

    public void RemoveObject(GameObject objectToRemove, float removeTime = 0)
    {
        LevelObject levelObject = LevelObjects.FirstOrDefault(levelobject => levelobject.gameObject == objectToRemove);

        if(levelObject != null)
        {
            levelObject.DestroyObject(removeTime);
            LevelObjects.Remove(levelObject);
        }
    }

    public void RemoveObject(LevelObject levelObjectToRemove, float removeTime = 0)
    {
        levelObjectToRemove.DestroyObject(removeTime);
        LevelObjects.Remove(levelObjectToRemove);
    }

    public void RemoveEntities(bool removePlayer = false)
    {
        if(LevelEntities.Count > 0)
        {
            List<Entity> entitiesToRemoveList = new List<Entity>();
            entitiesToRemoveList.AddRange(LevelEntities);

            foreach(Entity entity in entitiesToRemoveList)
            {
                if(entity is Player && !removePlayer) { continue; }
                RemoveEntity(entity);
            }
        }
    }

    public void RemoveEntity(Entity entityToRemove, float removeTime = 0)
    {
        if(LevelEntities.Contains(entityToRemove) && entityToRemove != null)
        {
            entityToRemove.DestroyEntity(removeTime);
            LevelEntities.Remove(entityToRemove);
        }
    }

    public T GetEntity<T>() where T : Entity
    {
        foreach(Entity entity in LevelEntities)
        {
            if(entity is T) { return (T)entity; }
        }

        return null;
    }

    public List<T> GetEntities<T>() where T : Entity
    {
        List<T> entityList = new List<T>();

        foreach(Entity entity in LevelEntities)
        {
            if(entity == null || entity.Equals(null)) { continue; }

            if(entity is T)
            {
                entityList.Add(entity as T);
            }
        }

        return entityList;
    }

    public List<T> GetEntities<T>(Vector2 origin, float range) where T : Entity
    {
        List<T> entityList = new List<T>();

        foreach(Entity entity in LevelEntities)
        {
            if(entity == null || entity.Equals(null)) { continue; }

            if(entity is T && Vector2.Distance(origin, entity.CenterOfMass) <= range)
            {
                entityList.Add(entity as T);
            }
        }

        return entityList;
    }

    public static GameObject LevelParent { get; set; } = null;
    public static GameObject LevelEntityParent { get; set; } = null;
    public static GameObject LevelObjectParent { get; set; } = null;
}
