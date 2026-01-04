//claude
//https://claude.ai/chat/4fda98fe-fe99-4655-96a9-2753a837083f

using UnityEngine;

public class HexBehaviour : MonoBehaviour
{
    public float TextureSpeed = 0.0f;
    private float speed = 0.0f;
    private MeshRenderer meshRenderer;
    private float modX = 0.0f;
    private float modY = 0.0f;

    // ADD THESE:
    private MaterialPropertyBlock propBlock;
    private Vector2 offset;
    private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");

    private void MoveTextureInit()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();
        propBlock = new MaterialPropertyBlock(); // Create once

        speed = Random.Range(0.8f, 1.2f) * TextureSpeed;
        modX = Random.Range(0.8f, 1.2f);
        modY = Random.Range(0.8f, 1.2f);

        // Initialize random offset
        offset = new Vector2(
            Random.Range(0.0f, 20.0f) * speed,
            Random.Range(0.0f, 20.0f) * speed
        );
    }

    private void MoveTexture()
    {
        float dir = Mathf.RoundToInt(0.3f * Time.timeSinceLevelLoad) % 2 == 0 ? 1.0f : -1.0f;

        offset.x += dir * modX * speed * Time.deltaTime;
        offset.y += dir * modY * speed * Time.deltaTime;

        // Get current property block, modify it, apply it
        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetVector(BaseMapST, new Vector4(1, 1, offset.x, offset.y));
        meshRenderer.SetPropertyBlock(propBlock);
    }

    void Start()
    {
        MoveTextureInit();
    }

    void Update()
    {
        MoveTexture();
    }
}

/*
using UnityEngine;

public class HexBehaviour : MonoBehaviour
{

    public float TextureSpeed = 0.0f;
    private float speed = 0.0f;
    private MeshRenderer meshRenderer;
    private float modX = 0.0f;
    private float modY = 0.0f;
    

    private void MoveTextureInit()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();
        speed = Random.Range(0.8f, 1.2f) * TextureSpeed;
        modX = Random.Range(0.8f, 1.2f);
        modY = Random.Range(0.8f, 1.2f);
        Vector2 offset = meshRenderer.material.GetTextureOffset("_BaseMap");
        offset.x += Random.Range(0.0f, 20.0f) * speed; 
        offset.y += Random.Range(0.0f, 20.0f) * speed; 
        meshRenderer.material.SetTextureOffset("_BaseMap", offset);
    }

    private void MoveTexture()
    {
        Vector2 offset = meshRenderer.material.GetTextureOffset("_BaseMap");

        float dir = 1.0f;

        if (Mathf.RoundToInt(0.3f * Time.timeSinceLevelLoad) % 2 == 0)
        {
            dir = 1.0f;
        }
        else
        {
            dir = -1.0f;
        }

        offset.x += dir * modX * speed * Time.deltaTime; 
        offset.y += dir * modY * speed * Time.deltaTime; 
        meshRenderer.material.SetTextureOffset("_BaseMap", offset);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MoveTextureInit();

    }

    // Update is called once per frame
    void Update()
    {
        MoveTexture();

    }
}
*/