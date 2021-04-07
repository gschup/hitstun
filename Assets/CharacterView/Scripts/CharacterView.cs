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
    private HitboxView collisionBoxView;
    private List<HitboxView> hitboxViews;
    private List<HitboxView> hurtboxViews;

    public void Awake() {
        sprites = new Dictionary<string, Sprite[]>();
        hitboxViews = new List<HitboxView>();
        hurtboxViews = new List<HitboxView>();
    }

    public void LoadResources(CharacterData _data) {
        data = _data;

        // load sprites from animation data and store them into dictionary
        foreach(KeyValuePair<string, Animation> kvp in data.animations) {
            string animationName = kvp.Key;
            Animation animation = kvp.Value;
            Sprite[] spriteArray = new Sprite[animation.distinctSprites];
            for (int i=0; i<animation.distinctSprites; i++) {
                spriteArray[i] = Resources.Load<Sprite>("Sprites/" + data.name + "/ANIMATIONS/" + animationName + "/" + animationName + "_" + i.ToString());
            }
            sprites.Add(kvp.Key, spriteArray);
        }

        // load sprites from attack data and store them into dictionary
        foreach(KeyValuePair<string, Attack> kvp in data.attacks) {
            string animationName = kvp.Key;
            Attack attack = kvp.Value;
            Sprite[] spriteArray = new Sprite[attack.distinctSprites];
            for (int i=0; i<attack.distinctSprites; i++) {
                spriteArray[i] = Resources.Load<Sprite>("Sprites/" + data.name + "/ATTACKS/" + animationName + "/" + animationName + "_" + i.ToString());
            }
            sprites.Add(kvp.Key, spriteArray);
        }
    }

    public void UpdateCharacterView(Character character, PlayerConnectionInfo info) {
        // display correct sprite based on state
        CharacterState currentState = character.state;
        Animation currentAnimation = character.isAttacking() ? data.attacks[currentState.ToString()] : data.animations[currentState.ToString()];
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

        // boxes
        if (showHitboxes) {
            // collisionBox
            if (collisionBoxView is null) {
                collisionBoxView = Instantiate(hitboxPrefab, transform);
                collisionBoxView.spriteRenderer.color = new Color(0f,1f,0f,.5f);
                collisionBoxView.spriteRenderer.sortingLayerName = "COLLISIONBOX";
            }
            collisionBoxView.setRect(viewX, viewY, zDistance, character.facingRight, currentAnimation.collisionBox);

            //hitboxes
            // deactivate all hitboxviews
            foreach (HitboxView hitboxView in hitboxViews) {
                hitboxView.spriteRenderer.enabled = false;
            }

            List<Box> hitboxes;
            if (character.GetHitBoxes(data, out hitboxes)) {
                int diff = hitboxes.Count - hitboxViews.Count;
                // instanciate additional hitboxviews, if needed
                if (diff > 0) {
                    for (int i=0; i<diff; i++) {
                        HitboxView hitboxView = Instantiate(hitboxPrefab, transform);
                        hitboxView.spriteRenderer.color = new Color(1f,0f,0f,.5f);
                        hitboxView.spriteRenderer.sortingLayerName = "HITBOX";
                        hitboxViews.Add(hitboxView);
                    }
                }
                // set the hitboxviews to the correct place
                foreach (HitboxView hitboxView in hitboxViews) {
                    hitboxView.spriteRenderer.enabled = true;
                    hitboxView.setRect(viewX, viewY, zDistance, character.facingRight, hitboxes[0].getCoords());
                    hitboxes.RemoveAt(0);
                    if (hitboxes.Count <= 0) break;                     
                }
            }

            //hurtboxes
            // deactivate all hurtboxviews
            foreach (HitboxView hurtboxView in hurtboxViews) {
                hurtboxView.spriteRenderer.enabled = false;
            }

            List<Box> hurtboxes;
            if (character.GetHurtBoxes(data, out hurtboxes)) {
                int diff = hurtboxes.Count - hurtboxViews.Count;
                // instanciate additional hurtboxViews, if needed
                if (diff > 0) {
                    for (int i=0; i<diff; i++) {
                        HitboxView hurtBoxView = Instantiate(hitboxPrefab, transform);
                        hurtBoxView.spriteRenderer.color = new Color(0f,0f,1f,.5f);
                        hurtBoxView.spriteRenderer.sortingLayerName = "HURTBOX";
                        hurtboxViews.Add(hurtBoxView);
                    }
                }
                // set the hurtboxViews to the correct place
                foreach (HitboxView hurtboxView in hurtboxViews) {
                    hurtboxView.spriteRenderer.enabled = true;
                    hurtboxView.setRect(viewX, viewY, zDistance, character.facingRight, hurtboxes[0].getCoords());
                    hurtboxes.RemoveAt(0);
                    if (hurtboxes.Count <= 0) break;                     
                }
            }
        }  
    }
}
