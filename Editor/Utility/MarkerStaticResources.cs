using UnityEngine;

namespace VRLabs.Marker
{
    public class MarkerStaticResources : MonoBehaviour
    {
        // PC FX Layers
        // Drawing Layer
        public static string DrawLayer = "a5637a89efa42104395141c9482783d8";

        // Blend Tree Layer
        public static string BlendTreeCombinedSize = "53617c500ba6e264b8a7af3b0f07d532"; // Brush and Eraser scaled together
        public static string BlendTreeNoSize = "5a00fa65aa5c4064394b000eae96ba98"; // No Brush or Eraser scaling available
        public static string BlendTreeSeparateSize = "98a58d518b916524bbff4b2e4ce83bc6"; // Brush and Eraser scaled separately

        // Space Layer
        public static string SpaceComplex = "d72fa00c54c20db49a926ce6af49dff2"; // The drawing can be attached to various body parts
        public static string SpaceSimple = "b4d43cb52ba16be4a998f071f3dc6543"; // The drawing can only be attached to the avatar root

        // Menu Interactions
        public static string MenuInteractionsNoSizeComplexSpace = "52eef6eccddb91c46b4f636b588f1d2e";
        public static string MenuInteractionsNoSizeSimpleSpace = "16e3d2bdba4e158479b6b359e7c86d2f";
        public static string MenuInteractionsCombinedSizeComplexSpace = "1b59a271cafb79b48b205ed96e2fff76";
        public static string MenuInteractionsCombinedSizeSimpleSpace = "fbe144fae2d259b4db1d2118cd8f1b06";
        public static string MenuInteractionsSeparateSizeSimpleSpace = "5b04c718ac520654780981adbc4c1a48";
        public static string MenuInteractionsSeparateSizeComplexSpace = "0770572dd81c0b748ae8523288c0a1f9";

        // Gesture Layers
        public static string GestureWithPen = "2a28c48a730ccc34981997ad9bee2d27";
        public static string GestureNoPen = "c4edf4c81174a7e4499301ecc8c61faf";

        // Menu Toggles
        public static string MenuToggles = "ebd70e98bd4275846b704d04ca9c0c4f";

        // Installation
        public static string PreviewAnimatorPen = "30dfc185ad9c69145a82b7d93fe8fd31";
        public static string PreviewAnimatorNoPen = "8cb1fc4d9f8fca04e86999388d582fb3";

        // Other
        public static string MarkerLogTag = "<color=\"#32CD32\">[VRLabs Marker]</color> ";
        public static string MarkerNoSizeMaterial = "d86b905765f9918419ff633bbfc4449f";
    }
}