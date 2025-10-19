using UnityEngine;
using Alteruna;

public class CubeSpawner : MonoBehaviour
{
    private Alteruna.Avatar _avatar;  // Fully qualified to avoid ambiguity with UnityEngine.Avatar
    private Spawner _spawner;

    [SerializeField] private int indexToSpawn = 0;
    [SerializeField] private LayerMask spawnLayer;

    void Awake()
    {
        _avatar = GetComponentInParent<Alteruna.Avatar>();  // Fully qualified
        GameObject networkManager = GameObject.FindGameObjectWithTag("NetworkManager");
        if (networkManager != null)
        {
            _spawner = networkManager.GetComponent<Spawner>();
        }
        else
        {
            Debug.LogError("No GameObject tagged 'NetworkManager' found! Add tag to your Multiplayer object.");
        }

        if (_avatar == null) Debug.LogError("Avatar component not found on player!");
        if (_spawner == null) Debug.LogError("Spawner component not found on NetworkManager!");
    }

    void Update()
    {
        if (_avatar == null || !_avatar.IsMe) return;  // Adjusted to IsMe; skip if not local player

        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnCube();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DespawnCube();
        }
    }

    void SpawnCube()
    {
        if (_spawner == null) return;
        Debug.Log("Spawning cube...");
        _spawner.Spawn(indexToSpawn, 
                       Camera.main.transform.position + Camera.main.transform.forward * 1.5f, 
                       Camera.main.transform.rotation, 
                       new Vector3(0.5f, 0.5f, 0.5f));
    }

    void DespawnCube()
    {
        if (_spawner == null) return;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, spawnLayer))
        {
            Debug.Log("Despawning: " + hit.transform.name);
            _spawner.Despawn(hit.transform.gameObject);
        }
        else
        {
            Debug.Log("No cube hit for despawn.");
        }
    }
}