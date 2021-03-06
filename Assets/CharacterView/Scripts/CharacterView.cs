using System;
using UnityEngine;
using System.Collections.Generic;

using HitstunConstants;

public class CharacterView : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Projector shadowProjector;
    public HitboxView hitboxPrefab;
    public ProjectileView projectilePrefab;
    public float zDistance = 2.0f;
    public float shadowSize = 0.5f;
    public float shadowOffset = -0.03f;
    public bool showHitboxes { get; set; }
    private CharacterData data;
    private Dictionary<string, Sprite[]> sprites;
    private HitboxView collisionBoxView;
    private HitboxView projectileBoxView;
    private ProjectileView projectileView;
    private List<HitboxView> hitboxViews;
    private List<HitboxView> hurtboxViews;

    public void Awake()
    {
        sprites = new Dictionary<string, Sprite[]>();
        hitboxViews = new List<HitboxView>();
        hurtboxViews = new List<HitboxView>();

        collisionBoxView = Instantiate(hitboxPrefab, transform);
        collisionBoxView.spriteRenderer.color = new Color(0f, 1f, 0f, .5f);
        collisionBoxView.spriteRenderer.sortingLayerName = "COLLISIONBOX";

        projectileView = Instantiate(projectilePrefab, transform);
        projectileView.spriteRenderer.sortingLayerName = "PROJECTILE";

        projectileBoxView = Instantiate(hitboxPrefab, transform);
        projectileBoxView.spriteRenderer.color = new Color(1f, 0f, 0f, .5f);
        projectileBoxView.spriteRenderer.sortingLayerName = "HITBOX";
    }

    public void LoadResources(CharacterData _data)
    {
        data = _data;

        // load sprites from animation data and store them into dictionary
        foreach (KeyValuePair<string, Animation> kvp in data.animations)
        {
            string animationName = kvp.Key;
            Animation animation = kvp.Value;
            Sprite[] spriteArray = new Sprite[animation.distinctSprites];
            for (int i = 0; i < animation.distinctSprites; i++)
            {
                spriteArray[i] = Resources.Load<Sprite>("Sprites/" + data.name + "/ANIMATIONS/" + animationName + "/" + animationName + "_" + i.ToString());
            }
            sprites.Add(kvp.Key, spriteArray);
        }

        // load sprites from attack data and store them into dictionary
        foreach (KeyValuePair<string, Attack> kvp in data.attacks)
        {
            string animationName = kvp.Key;
            Attack attack = kvp.Value;
            Sprite[] spriteArray = new Sprite[attack.distinctSprites];
            for (int i = 0; i < attack.distinctSprites; i++)
            {
                spriteArray[i] = Resources.Load<Sprite>("Sprites/" + data.name + "/ATTACKS/" + animationName + "/" + animationName + "_" + i.ToString());
            }
            sprites.Add(kvp.Key, spriteArray);
        }

        // load sprites from attack data and store them into dictionary
        foreach (KeyValuePair<string, ProjectileData> kvp in data.projectiles)
        {
            string animationName = kvp.Key;
            ProjectileData attack = kvp.Value;
            Sprite[] spriteArray = new Sprite[attack.distinctSprites];
            for (int i = 0; i < attack.distinctSprites; i++)
            {
                spriteArray[i] = Resources.Load<Sprite>("Sprites/" + data.name + "/PROJECTILES/" + animationName + "/" + animationName + "_" + i.ToString());
            }
            sprites.Add(kvp.Key, spriteArray);
        }
    }

    public void UpdateCharacterView(Character character, PlayerConnectionInfo info)
    {
        // display correct sprite based on state
        CharacterState currentState = character.state;
        Animation currentAnimation = character.isAttacking() ? data.attacks[currentState.ToString()] : data.animations[currentState.ToString()];
        int currentFrame = (int)character.framesInState % currentAnimation.totalFrames;
        int spriteIndex = 0;
        for (int i = 0; i < currentAnimation.frameDuration.Length; i++)
        {
            currentFrame -= currentAnimation.frameDuration[i];
            if (currentFrame <= 0)
            {
                spriteIndex = i;
                break;
            }
        }
        spriteRenderer.sprite = sprites[currentAnimation.animationName][spriteIndex % currentAnimation.totalFrames];

        // x and y position
        float viewX = ((character.position.x - Constants.BOUNDS_WIDTH / 2) / Constants.SCALE);
        float viewY = (character.position.y / Constants.SCALE);
        transform.position = new Vector3(viewX, viewY, zDistance);

        float maxY = Constants.BOUNDS_HEIGHT / Constants.SCALE;
        float normY = viewY / maxY;
        shadowProjector.orthographicSize = shadowSize * (1.0f - normY) * (1.0f - normY);

        // sprite facing direction
        spriteRenderer.flipX = character.facingRight;
        float flipShadow = (character.facingRight) ? shadowOffset : -shadowOffset;
        shadowProjector.transform.position = new Vector3(viewX + flipShadow, viewY + 2.0f, zDistance + 0.15f);

        // set correct drawing layer
        if (character.onTop)
        {
            spriteRenderer.sortingLayerName = "PLAYER_TOP";
        }
        else
        {
            spriteRenderer.sortingLayerName = "PLAYER_BOTTOM";
        }

        // projectile
        if (character.projectile.active)
        {
            projectileView.spriteRenderer.enabled = true;
            int index;
            if (character.projectile.activeSince < 2)
            {
                index = 0;
            }
            else
            {
                index = ((int)character.projectile.activeSince % (data.projectiles["FIREBALL"].distinctSprites - 1)) + 1;
            }
            projectileView.spriteRenderer.sprite = sprites["FIREBALL"][index];
            projectileView.spriteRenderer.flipX = character.projectile.facingRight;
            float projectileViewX = ((character.projectile.position.x - Constants.BOUNDS_WIDTH / 2) / Constants.SCALE);
            float projectileViewY = (character.projectile.position.y / Constants.SCALE);
            projectileView.transform.position = new Vector3(projectileViewX, projectileViewY, zDistance);

        }
        else
        {
            projectileView.spriteRenderer.enabled = false;
        }

        // boxes
        if (showHitboxes)
        {
            // collisionBox
            if (currentAnimation.collisionBox != null)
            {
                collisionBoxView.spriteRenderer.enabled = true;
                collisionBoxView.setRect(viewX, viewY, zDistance, character.facingRight, currentAnimation.collisionBox);
            }
            else
            {
                collisionBoxView.spriteRenderer.enabled = false;
            }

            // projectile
            if (character.projectile.active)
            {
                projectileBoxView.spriteRenderer.enabled = true;
                float projectileViewX = ((character.projectile.position.x - Constants.BOUNDS_WIDTH / 2) / Constants.SCALE);
                float projectileViewY = (character.projectile.position.y / Constants.SCALE);
                projectileBoxView.setRect(projectileViewX, projectileViewY, zDistance, character.projectile.facingRight, new int[]{-25,25,-100,100});
            }
            else
            {
                projectileBoxView.spriteRenderer.enabled = false;
            }


            //hitboxes
            // deactivate all hitboxviews
            foreach (HitboxView hitboxView in hitboxViews)
            {
                hitboxView.spriteRenderer.enabled = false;
            }

            if (character.hitBoxes.Count > 0)
            {
                int diff = character.hitBoxes.Count - hitboxViews.Count;
                // instanciate additional hitboxviews, if needed
                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        HitboxView hitboxView = Instantiate(hitboxPrefab, transform);
                        hitboxView.spriteRenderer.color = new Color(1f, 0f, 0f, .5f);
                        hitboxView.spriteRenderer.sortingLayerName = "HITBOX";
                        hitboxView.spriteRenderer.enabled = false;
                        hitboxViews.Add(hitboxView);
                    }
                }

                // set the hitboxviews to the correct place
                for (int i = 0; i < character.hitBoxes.Count; i++)
                {
                    if (!character.hitBoxes[i].enabled) continue;
                    hitboxViews[i].setRect(viewX, viewY, zDistance, character.facingRight, character.hitBoxes[i].GetCoords());
                    hitboxViews[i].spriteRenderer.enabled = true;
                }
            }

            //hurtboxes
            // deactivate all hurtboxviews
            foreach (HitboxView hurtboxView in hurtboxViews)
            {
                hurtboxView.spriteRenderer.enabled = false;
            }

            List<Box> hurtboxes;
            if (character.GetHurtBoxes(data, out hurtboxes))
            {
                int diff = hurtboxes.Count - hurtboxViews.Count;
                // instanciate additional hurtboxViews, if needed
                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        HitboxView hurtBoxView = Instantiate(hitboxPrefab, transform);
                        hurtBoxView.spriteRenderer.color = new Color(0f, 0f, 1f, .5f);
                        hurtBoxView.spriteRenderer.sortingLayerName = "HURTBOX";
                        hurtboxViews.Add(hurtBoxView);
                    }
                }
                // set the hurtboxViews to the correct place
                foreach (HitboxView hurtboxView in hurtboxViews)
                {
                    hurtboxView.spriteRenderer.enabled = true;
                    hurtboxView.setRect(viewX, viewY, zDistance, character.facingRight, hurtboxes[0].GetCoords());
                    hurtboxes.RemoveAt(0);
                    if (hurtboxes.Count <= 0) break;
                }
            }
        }
        else
        {
            // deactivate collisionboxview
            collisionBoxView.spriteRenderer.enabled = false;
            projectileBoxView.spriteRenderer.enabled = false;
            // deactivate all hitboxviews
            foreach (HitboxView hitboxView in hitboxViews)
            {
                hitboxView.spriteRenderer.enabled = false;
            }
            // deactivate all hurtboxviews
            foreach (HitboxView hurtboxView in hurtboxViews)
            {
                hurtboxView.spriteRenderer.enabled = false;
            }
        }
    }
}
