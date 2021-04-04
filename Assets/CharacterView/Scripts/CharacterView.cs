using System;
using UnityEngine;
using System.Collections.Generic;

using HitstunConstants;

public class CharacterView : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public Projector shadowProjector;
    public HitboxView hitboxPrefab;
    public float zDistance = 2.0f;
    public float shadowSize = 0.5f;
    public float shadowOffset = -0.03f;
    public bool showHitboxes { get; set; }
    private CharacterData data;
    private Dictionary<string, Sprite[]> sprites;
    private HitboxView collisionBox;

    public void LoadResources(CharacterData _data) {
        data = _data;
        sprites = new Dictionary<string, Sprite[]>();

        // load sprites from animation data and store them into dictionary
        foreach(KeyValuePair<string, Animation> kvp in data.animations) {
            string animationName = kvp.Key;
            Animation animation = kvp.Value;
            Sprite[] spriteArray = new Sprite[animation.distinctSprites];
            for (int i=0; i<animation.distinctSprites; i++) {
                spriteArray[i] = Resources.Load<Sprite>("Sprites/" + data.name + "/" + animationName + "/" + animationName + "_" + i.ToString());
            }
            sprites.Add(kvp.Key, spriteArray);
        }
    }

    public void UpdateCharacterView(Character character, PlayerConnectionInfo info) {
        // display correct sprite based on state
        CharacterState currentState = character.state;
        Animation currentAnimation = data.animations[currentState.ToString()];
        int currentFrame = (int)character.framesInState % currentAnimation.totalFrames;
        int spriteIndex = 0;
        for (int i=0; i<currentAnimation.frameDuration.Length; i++) {
            currentFrame -= currentAnimation.frameDuration[i];
            if (currentFrame <= 0) {
                spriteIndex = i;
                break;
            }
        }
        spriteRenderer.sprite = sprites[currentAnimation.animationName][spriteIndex % currentAnimation.totalFrames];

        // x and y position
        float viewX = ((character.position.x - Constants.BOUNDS_WIDTH/2) / Constants.SCALE);
        float viewY = (character.position.y / Constants.SCALE);
        transform.position = new Vector3(viewX, viewY, zDistance);

        float maxY = Constants.BOUNDS_HEIGHT / Constants.SCALE;
        float normY = viewY / maxY;
        shadowProjector.orthographicSize = shadowSize * (1.0f-normY) * (1.0f-normY);

        // sprite facing direction
        spriteRenderer.flipX = character.facingRight;
        float flipShadow = (character.facingRight) ? shadowOffset : -shadowOffset;
        shadowProjector.transform.position = new Vector3(viewX+flipShadow, viewY+2.0f, zDistance+0.15f);

        // set correct drawing layer
        if (character.onTop) {
            spriteRenderer.sortingLayerName = "PLAYER_TOP";
        } else {
            spriteRenderer.sortingLayerName = "PLAYER_BOTTOM";
        }

        // hitboxes
        if (showHitboxes) {
            if (collisionBox is null) {
                collisionBox = Instantiate(hitboxPrefab, transform);
                collisionBox.spriteRenderer.color = new Color(0f,1f,0f,.5f);
            }
            collisionBox.setRect(viewX, viewY, zDistance, character.facingRight, currentAnimation.collisionBox);
        }  
    }
}
