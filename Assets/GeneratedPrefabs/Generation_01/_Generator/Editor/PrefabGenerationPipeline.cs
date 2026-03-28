#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GeneratedPrefabs.Generation01.Editor
{
    public static class PrefabGenerationPipeline
    {
        private struct DesignRect
        {
            public readonly float CenterX;
            public readonly float CenterY;
            public readonly float Width;
            public readonly float Height;

            public DesignRect(float centerX, float centerY, float width, float height)
            {
                CenterX = centerX;
                CenterY = centerY;
                Width = width;
                Height = height;
            }
        }

        private sealed class CardSpec
        {
            public string CardName;
            public float CardX;
            public float CardY;
            public string PanelName;
            public float PanelX;
            public float PanelY;
            public string ActionButtonName;
            public string LabelName;
            public string LabelText;
            public float LabelX;
            public float LabelY;
            public float LockX;
            public float LockY;
            public List<RewardItemSpec> RewardItems;
        }

        private sealed class RewardItemSpec
        {
            public string Name;
            public float X;
            public float Y;
            public float Width;
            public float Height;
            public string Value;
        }

        public static void Generate(PrefabGenerationProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("[Generation_01] PrefabGenerationProfile is null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(profile.RootFolder))
            {
                Debug.LogError("[Generation_01] Root folder is empty in profile.");
                return;
            }

            var rootFolder = profile.RootFolder.Replace('\\', '/');
            if (!rootFolder.StartsWith("Assets/GeneratedPrefabs/Generation_01", StringComparison.Ordinal))
            {
                Debug.LogError($"[Generation_01] Root folder '{rootFolder}' is outside the allowed workspace root.");
                return;
            }

            EnsureFolder(rootFolder);
            EnsureFolder($"{rootFolder}/RewardItem");
            EnsureFolder($"{rootFolder}/ActionButton");
            EnsureFolder($"{rootFolder}/RewardPanel");
            EnsureFolder($"{rootFolder}/OfferCard");
            EnsureFolder($"{rootFolder}/CountdownWidget");
            EnsureFolder($"{rootFolder}/BeachTreasuresScreen");

            var tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/bangerscyrillic SDF.asset");

            var rewardItemPrefab = GenerateRewardItemPrefab(rootFolder, tmpFont);
            var actionButtonPrefab = GenerateActionButtonPrefab(rootFolder, tmpFont);
            var rewardPanelPrefab = GenerateRewardPanelPrefab(rootFolder);
            var offerCardPrefab = GenerateOfferCardPrefab(rootFolder, rewardPanelPrefab, actionButtonPrefab);
            var countdownWidgetPrefab = GenerateCountdownWidgetPrefab(rootFolder, tmpFont);
            var screenPrefab = GenerateBeachTreasuresScreenPrefab(rootFolder, profile.BeachTreasuresScreenPrefabName, countdownWidgetPrefab, offerCardPrefab, rewardItemPrefab, tmpFont);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Generation_01] Generation completed successfully.");
            Debug.Log($"[Generation_01] Created or updated prefab: {AssetDatabase.GetAssetPath(rewardItemPrefab)}");
            Debug.Log($"[Generation_01] Created or updated prefab: {AssetDatabase.GetAssetPath(actionButtonPrefab)}");
            Debug.Log($"[Generation_01] Created or updated prefab: {AssetDatabase.GetAssetPath(rewardPanelPrefab)}");
            Debug.Log($"[Generation_01] Created or updated prefab: {AssetDatabase.GetAssetPath(offerCardPrefab)}");
            Debug.Log($"[Generation_01] Created or updated prefab: {AssetDatabase.GetAssetPath(countdownWidgetPrefab)}");
            Debug.Log($"[Generation_01] Created or updated prefab: {AssetDatabase.GetAssetPath(screenPrefab)}");
        }

        private static GameObject GenerateRewardItemPrefab(string rootFolder, TMP_FontAsset tmpFont)
        {
            var root = CreateUiNode("RewardItem", null, typeof(Image));
            var rootRt = root.GetComponent<RectTransform>();
            ApplyFixedLayout(rootRt, new DesignRect(0f, 0f, 150f, 116f));

            var valueGo = CreateUiNode("RewardValueText", root.transform, typeof(TextMeshProUGUI));
            var valueRt = valueGo.GetComponent<RectTransform>();
            ApplyFixedLayout(valueRt, new DesignRect(0f, 0f, 130f, 48f));

            var valueTmp = valueGo.GetComponent<TextMeshProUGUI>();
            valueTmp.text = "x1";
            valueTmp.alignment = TextAlignmentOptions.Center;
            ApplyPreferredTmpFont(valueTmp, tmpFont);

            var view = root.AddComponent<RewardItemView>();
            view.Bind(root.GetComponent<Image>(), valueTmp);

            return SavePrefab(root, $"{rootFolder}/RewardItem/RewardItemPrefab.prefab");
        }

        private static GameObject GenerateActionButtonPrefab(string rootFolder, TMP_FontAsset tmpFont)
        {
            var root = CreateUiNode("ActionButton", null, typeof(Image), typeof(Button));
            var rootRt = root.GetComponent<RectTransform>();
            ApplyFixedLayout(rootRt, new DesignRect(0f, 0f, 258f, 88f));

            var labelGo = CreateUiNode("ActionLabelText", root.transform, typeof(TextMeshProUGUI));
            var labelRt = labelGo.GetComponent<RectTransform>();
            ApplyFixedLayout(labelRt, new DesignRect(0f, 0f, 132f, 46f));

            var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
            labelTmp.text = "Free";
            labelTmp.alignment = TextAlignmentOptions.Center;
            ApplyPreferredTmpFont(labelTmp, tmpFont);

            var lockGo = CreateUiNode("LockBadge", root.transform, typeof(Image));
            var lockRt = lockGo.GetComponent<RectTransform>();
            ApplyFixedLayout(lockRt, new DesignRect(72f, -16f, 58f, 62f));

            var view = root.AddComponent<ActionButtonView>();
            view.Bind(labelTmp, lockGo.GetComponent<Image>());

            return SavePrefab(root, $"{rootFolder}/ActionButton/ActionButtonPrefab.prefab");
        }

        private static GameObject GenerateRewardPanelPrefab(string rootFolder)
        {
            var root = CreateUiNode("RewardPanel", null, typeof(Image));
            var rootRt = root.GetComponent<RectTransform>();
            ApplyFixedLayout(rootRt, new DesignRect(0f, 0f, 342f, 150f));

            var listGo = CreateUiNode("RewardItemPrefabList", root.transform);
            var listRt = listGo.GetComponent<RectTransform>();
            StretchToParent(listRt);

            var view = root.AddComponent<RewardPanelView>();
            view.Bind(root.GetComponent<Image>(), listRt, new List<RewardItemView>());

            return SavePrefab(root, $"{rootFolder}/RewardPanel/RewardPanelPrefab.prefab");
        }

        private static GameObject GenerateOfferCardPrefab(string rootFolder, GameObject rewardPanelPrefab, GameObject actionButtonPrefab)
        {
            var root = CreateUiNode("OfferCard", null, typeof(Image));
            var rootRt = root.GetComponent<RectTransform>();
            ApplyFixedLayout(rootRt, new DesignRect(0f, 0f, 392f, 344f));

            var rewardPanelParent = CreateUiNode("RewardPanelPrefabParent", root.transform);
            var rewardPanelParentRt = rewardPanelParent.GetComponent<RectTransform>();
            StretchToParent(rewardPanelParentRt);

            var actionButtonParent = CreateUiNode("ActionButtonPrefabParent", root.transform);
            var actionButtonParentRt = actionButtonParent.GetComponent<RectTransform>();
            StretchToParent(actionButtonParentRt);

            var rewardPanelInstance = InstantiatePrefabUnderParent(rewardPanelPrefab, rewardPanelParent.transform, "RewardPanel_01");
            var actionButtonInstance = InstantiatePrefabUnderParent(actionButtonPrefab, actionButtonParent.transform, "ActionButton_01");

            var rewardPanelView = rewardPanelInstance.GetComponent<RewardPanelView>();
            var actionButtonView = actionButtonInstance.GetComponent<ActionButtonView>();

            var view = root.AddComponent<OfferCardView>();
            view.Bind(
                root.GetComponent<Image>(),
                rewardPanelParentRt,
                rewardPanelView,
                actionButtonParentRt,
                actionButtonView);

            return SavePrefab(root, $"{rootFolder}/OfferCard/OfferCardPrefab.prefab");
        }

        private static GameObject GenerateCountdownWidgetPrefab(string rootFolder, TMP_FontAsset tmpFont)
        {
            var root = CreateUiNode("CountdownWidget", null, typeof(Image));
            var rootRt = root.GetComponent<RectTransform>();
            ApplyFixedLayout(rootRt, new DesignRect(0f, 0f, 266f, 94f));

            var iconGo = CreateUiNode("ClockIcon", root.transform, typeof(Image));
            ApplyFixedLayout(iconGo.GetComponent<RectTransform>(), new DesignRect(-47f, 0f, 62f, 62f));

            var timeGo = CreateUiNode("TimeText", root.transform, typeof(TextMeshProUGUI));
            ApplyFixedLayout(timeGo.GetComponent<RectTransform>(), new DesignRect(65f, 0f, 150f, 52f));

            var timeTmp = timeGo.GetComponent<TextMeshProUGUI>();
            timeTmp.text = "00:00";
            timeTmp.alignment = TextAlignmentOptions.Center;
            ApplyPreferredTmpFont(timeTmp, tmpFont);

            var view = root.AddComponent<CountdownWidgetView>();
            view.Bind(iconGo.GetComponent<Image>(), timeTmp);

            return SavePrefab(root, $"{rootFolder}/CountdownWidget/CountdownWidgetPrefab.prefab");
        }

        private static GameObject GenerateBeachTreasuresScreenPrefab(
            string rootFolder,
            string prefabName,
            GameObject countdownWidgetPrefab,
            GameObject offerCardPrefab,
            GameObject rewardItemPrefab,
            TMP_FontAsset tmpFont)
        {
            var root = CreateUiNode("BeachTreasuresScreenRoot", null, typeof(CanvasGroup));
            var rootRt = root.GetComponent<RectTransform>();
            StretchFullScreen(rootRt);

            var screenView = root.AddComponent<BeachTreasuresScreenView>();

            var backgroundArt = CreateUiNode("BackgroundArt", root.transform, typeof(Image));
            ApplyScreenPosition(backgroundArt.GetComponent<RectTransform>(), new DesignRect(540f, 960f, 1080f, 1920f), 540f, 960f);

            var topVignetteOverlay = CreateUiNode("TopVignetteOverlay", root.transform, typeof(Image));
            ApplyScreenPosition(topVignetteOverlay.GetComponent<RectTransform>(), new DesignRect(540f, 960f, 1080f, 1920f), 540f, 960f);

            var countdownParent = CreateUiNode("CountdownWidgetPrefabParent", root.transform);
            var countdownParentRt = countdownParent.GetComponent<RectTransform>();
            ApplyTopLeftAnchored(countdownParentRt, 266f, 94f, 43f, 61f);

            var countdownInstance = InstantiatePrefabUnderParent(countdownWidgetPrefab, countdownParent.transform, "CountdownWidget");
            StretchToParent(countdownInstance.GetComponent<RectTransform>());

            var closeButton = CreateUiNode("CloseButton", root.transform, typeof(Image), typeof(Button));
            ApplyTopRightAnchored(closeButton.GetComponent<RectTransform>(), 106f, 106f, 47f, 55f);

            var titleBlock = CreateUiNode("TitleBlock", root.transform);
            var titleRect = titleBlock.GetComponent<RectTransform>();
            ApplyScreenPosition(titleRect, new DesignRect(540f, 370f, 820f, 300f), 540f, 960f);

            var chestIcon = CreateUiNode("ChestIcon", titleBlock.transform, typeof(Image));
            ApplyLocalPosition(chestIcon.GetComponent<RectTransform>(), new DesignRect(540f, 294f, 228f, 154f), 540f, 370f);

            var titleText = CreateUiNode("TitleText", titleBlock.transform, typeof(TextMeshProUGUI));
            var titleTextTmp = titleText.GetComponent<TextMeshProUGUI>();
            titleTextTmp.text = "Beach Treasures";
            titleTextTmp.alignment = TextAlignmentOptions.Center;
            ApplyPreferredTmpFont(titleTextTmp, tmpFont);
            ApplyLocalPosition(titleText.GetComponent<RectTransform>(), new DesignRect(540f, 374f, 760f, 98f), 540f, 370f);

            var subtitleText = CreateUiNode("SubtitleText", titleBlock.transform, typeof(TextMeshProUGUI));
            var subtitleTextTmp = subtitleText.GetComponent<TextMeshProUGUI>();
            subtitleTextTmp.text = "Collect rewards";
            subtitleTextTmp.alignment = TextAlignmentOptions.Center;
            ApplyPreferredTmpFont(subtitleTextTmp, tmpFont);
            ApplyLocalPosition(subtitleText.GetComponent<RectTransform>(), new DesignRect(540f, 454f, 760f, 56f), 540f, 370f);

            var flowConnectorLayer = CreateUiNode("FlowConnectorLayer", root.transform);
            var flowRect = flowConnectorLayer.GetComponent<RectTransform>();
            StretchFullScreen(flowRect);

            CreateArrow(flowConnectorLayer.transform, "Row01Arrow", 540f, 880f, 84f, 66f);
            CreateArrow(flowConnectorLayer.transform, "MidDownArrow01", 540f, 1112f, 62f, 48f);
            CreateArrow(flowConnectorLayer.transform, "Row02Arrow", 540f, 1272f, 84f, 66f);
            CreateArrow(flowConnectorLayer.transform, "MidDownArrow02", 540f, 1504f, 62f, 48f);
            CreateArrow(flowConnectorLayer.transform, "Row03Arrow", 540f, 1666f, 84f, 66f);

            var offerCardListRoot = CreateUiNode("OfferCardListRoot", root.transform);
            var offerCardListRootRt = offerCardListRoot.GetComponent<RectTransform>();
            ApplyBottomStretchWithFixedHeight(offerCardListRootRt, 52f, 52f, 186f, 1260f);

            var offerCardPrefabList = CreateUiNode("OfferCardPrefabList", offerCardListRoot.transform);
            var offerCardPrefabListRt = offerCardPrefabList.GetComponent<RectTransform>();
            StretchToParent(offerCardPrefabListRt);

            var cards = BuildCardSpecs();
            var createdCardViews = new List<OfferCardView>(cards.Count);

            foreach (var card in cards)
            {
                var cardInstance = InstantiatePrefabUnderParent(offerCardPrefab, offerCardPrefabList.transform, card.CardName);
                var cardRt = cardInstance.GetComponent<RectTransform>();
                ApplyLocalPosition(cardRt, new DesignRect(card.CardX, card.CardY, 392f, 344f), 540f, 1104f);

                var cardView = cardInstance.GetComponent<OfferCardView>();
                ConfigureOfferCard(cardView, card, rewardItemPrefab, tmpFont);
                createdCardViews.Add(cardView);
            }

            var countdownWidgetView = countdownInstance.GetComponent<CountdownWidgetView>();
            var closeButtonView = closeButton.GetComponent<Button>();
            screenView.Bind(countdownParentRt, countdownWidgetView, closeButtonView, offerCardListRootRt, createdCardViews);

            var safePrefabName = string.IsNullOrWhiteSpace(prefabName) ? "BeachTreasuresScreenPrefab" : prefabName;
            return SavePrefab(root, $"{rootFolder}/BeachTreasuresScreen/{safePrefabName}.prefab");
        }

        private static void ConfigureOfferCard(OfferCardView cardView, CardSpec cardSpec, GameObject rewardItemPrefab, TMP_FontAsset tmpFont)
        {
            var cardImage = cardView.GetComponent<Image>();

            var rewardPanelView = cardView.RewardPanel;
            var rewardPanelRt = rewardPanelView.GetComponent<RectTransform>();
            rewardPanelView.name = cardSpec.PanelName;
            ApplyLocalPosition(rewardPanelRt, new DesignRect(cardSpec.PanelX, cardSpec.PanelY, 342f, 150f), cardSpec.CardX, cardSpec.CardY);

            var rewardItems = new List<RewardItemView>(cardSpec.RewardItems.Count);
            for (var i = rewardPanelView.RewardItemListTr.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(rewardPanelView.RewardItemListTr.GetChild(i).gameObject);
            }

            foreach (var itemSpec in cardSpec.RewardItems)
            {
                var itemInstance = InstantiatePrefabUnderParent(rewardItemPrefab, rewardPanelView.RewardItemListTr, itemSpec.Name);
                var itemRt = itemInstance.GetComponent<RectTransform>();
                ApplyLocalPosition(itemRt, new DesignRect(itemSpec.X, itemSpec.Y, itemSpec.Width, itemSpec.Height), cardSpec.PanelX, cardSpec.PanelY);

                var itemView = itemInstance.GetComponent<RewardItemView>();
                itemView.RewardValueTmp.text = itemSpec.Value;
                ApplyPreferredTmpFont(itemView.RewardValueTmp, tmpFont);
                rewardItems.Add(itemView);
            }

            rewardPanelView.Bind(rewardPanelView.GetComponent<Image>(), rewardPanelView.RewardItemListTr, rewardItems);

            var actionButtonView = cardView.ActionButton;
            actionButtonView.name = cardSpec.ActionButtonName;
            ApplyBottomStretchWithFixedHeight(actionButtonView.GetComponent<RectTransform>(), 67f, 67f, 24f, 88f);

            var labelTmp = actionButtonView.ActionLabelTmp;
            labelTmp.gameObject.name = cardSpec.LabelName;
            labelTmp.text = cardSpec.LabelText;
            ApplyPreferredTmpFont(labelTmp, tmpFont);

            const float actionLabelWidth = 132f;
            var actionButtonCenterY = cardSpec.CardY + 104f;
            ApplyLocalPosition(labelTmp.rectTransform, new DesignRect(cardSpec.LabelX, cardSpec.LabelY, actionLabelWidth, 46f), cardSpec.CardX, actionButtonCenterY);

            var lockBadgeRt = actionButtonView.LockBadgeImg.rectTransform;
            lockBadgeRt.gameObject.name = "LockBadge";
            ApplyLocalPosition(lockBadgeRt, new DesignRect(cardSpec.LockX, cardSpec.LockY, 58f, 62f), cardSpec.CardX, actionButtonCenterY);

            actionButtonView.Bind(labelTmp, actionButtonView.LockBadgeImg);
            cardView.Bind(cardImage, cardView.RewardPanelParentTr, rewardPanelView, cardView.ActionButtonParentTr, actionButtonView);
        }

        private static List<CardSpec> BuildCardSpecs()
        {
            return new List<CardSpec>
            {
                new CardSpec
                {
                    CardName = "OfferCard_01_Paid",
                    CardX = 280f,
                    CardY = 770f,
                    PanelName = "RewardPanel_01",
                    PanelX = 280f,
                    PanelY = 700f,
                    ActionButtonName = "ActionButton_01_Price",
                    LabelName = "ActionLabel_399",
                    LabelText = "399",
                    LabelX = 280f,
                    LabelY = 840f,
                    LockX = 352f,
                    LockY = 856f,
                    RewardItems = new List<RewardItemSpec>
                    {
                        new RewardItemSpec { Name = "RewardItem_01_Coins", X = 222f, Y = 700f, Width = 150f, Height = 116f, Value = "Coins" },
                        new RewardItemSpec { Name = "RewardItem_02_Infinity1h", X = 338f, Y = 700f, Width = 150f, Height = 116f, Value = "1h" }
                    }
                },
                new CardSpec
                {
                    CardName = "OfferCard_02_Free",
                    CardX = 800f,
                    CardY = 770f,
                    PanelName = "RewardPanel_02",
                    PanelX = 800f,
                    PanelY = 700f,
                    ActionButtonName = "ActionButton_02_Free",
                    LabelName = "ActionLabel_Free",
                    LabelText = "Free",
                    LabelX = 800f,
                    LabelY = 840f,
                    LockX = 872f,
                    LockY = 856f,
                    RewardItems = new List<RewardItemSpec>
                    {
                        new RewardItemSpec { Name = "RewardItem_01_Coins", X = 742f, Y = 700f, Width = 150f, Height = 116f, Value = "Coins" },
                        new RewardItemSpec { Name = "RewardItem_02_Snowflake", X = 858f, Y = 700f, Width = 150f, Height = 116f, Value = "Snow" }
                    }
                },
                new CardSpec
                {
                    CardName = "OfferCard_03_Free",
                    CardX = 280f,
                    CardY = 1165f,
                    PanelName = "RewardPanel_03",
                    PanelX = 280f,
                    PanelY = 1095f,
                    ActionButtonName = "ActionButton_03_Free",
                    LabelName = "ActionLabel_Free",
                    LabelText = "Free",
                    LabelX = 280f,
                    LabelY = 1235f,
                    LockX = 352f,
                    LockY = 1251f,
                    RewardItems = new List<RewardItemSpec>
                    {
                        new RewardItemSpec { Name = "RewardItem_01_Compass", X = 222f, Y = 1095f, Width = 150f, Height = 116f, Value = "Compass" },
                        new RewardItemSpec { Name = "RewardItem_02_Potion", X = 338f, Y = 1095f, Width = 150f, Height = 116f, Value = "Potion" }
                    }
                },
                new CardSpec
                {
                    CardName = "OfferCard_04_Free",
                    CardX = 800f,
                    CardY = 1165f,
                    PanelName = "RewardPanel_04",
                    PanelX = 800f,
                    PanelY = 1095f,
                    ActionButtonName = "ActionButton_04_Free",
                    LabelName = "ActionLabel_Free",
                    LabelText = "Free",
                    LabelX = 800f,
                    LabelY = 1235f,
                    LockX = 872f,
                    LockY = 1251f,
                    RewardItems = new List<RewardItemSpec>
                    {
                        new RewardItemSpec { Name = "RewardItem_01_Coins", X = 800f, Y = 1095f, Width = 150f, Height = 116f, Value = "Coins" }
                    }
                },
                new CardSpec
                {
                    CardName = "OfferCard_05_Free",
                    CardX = 280f,
                    CardY = 1560f,
                    PanelName = "RewardPanel_05",
                    PanelX = 280f,
                    PanelY = 1490f,
                    ActionButtonName = "ActionButton_05_Free",
                    LabelName = "ActionLabel_Free",
                    LabelText = "Free",
                    LabelX = 280f,
                    LabelY = 1630f,
                    LockX = 352f,
                    LockY = 1646f,
                    RewardItems = new List<RewardItemSpec>
                    {
                        new RewardItemSpec { Name = "RewardItem_01_Coins", X = 208f, Y = 1496f, Width = 130f, Height = 106f, Value = "Coins" },
                        new RewardItemSpec { Name = "RewardItem_02_Skullx3", X = 280f, Y = 1462f, Width = 110f, Height = 94f, Value = "x3" },
                        new RewardItemSpec { Name = "RewardItem_03_Snowflakex2", X = 352f, Y = 1496f, Width = 130f, Height = 106f, Value = "x2" }
                    }
                },
                new CardSpec
                {
                    CardName = "OfferCard_06_Free",
                    CardX = 800f,
                    CardY = 1560f,
                    PanelName = "RewardPanel_06",
                    PanelX = 800f,
                    PanelY = 1490f,
                    ActionButtonName = "ActionButton_06_Free",
                    LabelName = "ActionLabel_Free",
                    LabelText = "Free",
                    LabelX = 800f,
                    LabelY = 1630f,
                    LockX = 872f,
                    LockY = 1646f,
                    RewardItems = new List<RewardItemSpec>
                    {
                        new RewardItemSpec { Name = "RewardItem_01_Coins", X = 742f, Y = 1496f, Width = 130f, Height = 106f, Value = "Coins" },
                        new RewardItemSpec { Name = "RewardItem_02_Potionx1", X = 800f, Y = 1462f, Width = 110f, Height = 94f, Value = "x1" },
                        new RewardItemSpec { Name = "RewardItem_03_Infinity1h", X = 858f, Y = 1496f, Width = 130f, Height = 106f, Value = "1h" }
                    }
                }
            };
        }

        private static void CreateArrow(Transform parent, string name, float x, float y, float width, float height)
        {
            var arrow = CreateUiNode(name, parent, typeof(Image));
            ApplyScreenPosition(arrow.GetComponent<RectTransform>(), new DesignRect(x, y, width, height), 540f, 960f);
        }

        private static GameObject CreateUiNode(string name, Transform parent, params Type[] extraComponents)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            if (parent != null)
            {
                rt.SetParent(parent, false);
            }

            if (extraComponents != null)
            {
                foreach (var componentType in extraComponents)
                {
                    if (componentType != null && go.GetComponent(componentType) == null)
                    {
                        go.AddComponent(componentType);
                    }
                }
            }

            return go;
        }

        private static GameObject InstantiatePrefabUnderParent(GameObject prefab, Transform parent, string objectName)
        {
            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException("Failed to instantiate prefab instance.");
            }

            instance.name = objectName;
            var rt = instance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
            }

            return instance;
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            var normalizedPath = path.Replace('\\', '/');
            var folder = normalizedPath.Substring(0, normalizedPath.LastIndexOf('/'));
            EnsureFolder(folder);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, normalizedPath, out var success);
            if (!success)
            {
                Debug.LogError($"[Generation_01] Failed to save prefab at path: {normalizedPath}");
            }

            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void EnsureFolder(string assetFolderPath)
        {
            var normalized = assetFolderPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            var parts = normalized.Split('/');
            if (parts.Length < 2 || !string.Equals(parts[0], "Assets", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Folder path must be under Assets: {normalized}");
            }

            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void ApplyPreferredTmpFont(TextMeshProUGUI tmp, TMP_FontAsset preferredFont)
        {
            if (tmp == null || preferredFont == null)
            {
                return;
            }

            tmp.font = preferredFont;
        }

        private static void StretchFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static void ApplyBottomStretchWithFixedHeight(RectTransform rt, float left, float right, float bottom, float height)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, bottom + height);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, bottom + (height * 0.5f));
        }

        private static void ApplyTopLeftAnchored(RectTransform rt, float width, float height, float left, float top)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(left + (width * 0.5f), -top - (height * 0.5f));
        }

        private static void ApplyTopRightAnchored(RectTransform rt, float width, float height, float right, float top)
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(-right - (width * 0.5f), -top - (height * 0.5f));
        }

        private static void ApplyFixedLayout(RectTransform rt, DesignRect rect)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(rect.Width, rect.Height);
            rt.anchoredPosition = new Vector2(rect.CenterX, rect.CenterY);
        }

        private static void ApplyScreenPosition(RectTransform rt, DesignRect rect, float parentCenterX, float parentCenterY)
        {
            ApplyLocalPosition(rt, rect, parentCenterX, parentCenterY);
        }

        private static void ApplyLocalPosition(RectTransform rt, DesignRect rect, float parentCenterX, float parentCenterY)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(rect.Width, rect.Height);

            var localX = rect.CenterX - parentCenterX;
            var localY = parentCenterY - rect.CenterY;
            rt.anchoredPosition = new Vector2(localX, localY);
        }
    }
}
#endif
