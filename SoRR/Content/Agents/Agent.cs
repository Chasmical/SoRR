using System.Collections.Generic;
using UnityEngine;

namespace SoRR
{
    public abstract class Agent : Entity
    {
        public AgentMetadata Metadata { get; private set; } = null!;

        internal void InitialSetup(AgentMetadata metadata)
        {
            Metadata = metadata;
        }

    }
    public sealed class ResistanceLeader : Agent
    {
        [Inject] private readonly Dir8SpriteRenderer spriteRenderer = null!;

        public void Start()
        {
            spriteRenderer.SpriteName = $"Agents/{nameof(ResistanceLeader)}";
        }

        public void Update()
        {
            if (IsOwner)
            {
                Vector2 vector = Vector2.zero;
                if (Input.GetKey(KeyCode.W)) vector.y++;
                if (Input.GetKey(KeyCode.A)) vector.x--;
                if (Input.GetKey(KeyCode.S)) vector.y--;
                if (Input.GetKey(KeyCode.D)) vector.x++;
                transform.transform.Translate(vector * Time.deltaTime);
            }
        }

    }
}
