using UnityEngine;

namespace SoRR
{
    public abstract class UiElementBase : Injectable
    {
        [Inject] public readonly RectTransform rect = null!;

    }
}
