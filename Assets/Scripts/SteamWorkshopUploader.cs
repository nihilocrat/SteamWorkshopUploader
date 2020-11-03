using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

using Steamworks;
using TinyJSON;


public class SteamWorkshopUploader : MonoBehaviour
{
    public const int version = 8;

    public Text versionText;
    public Text statusText;
    public Slider progressBar;

    public RectTransform packListRoot;
    public GameObject packListButtonPrefab;

    [Header("ModPack Interface")]
    public RectTransform currentItemPanel;
    public Text submitButtonText;
    public Text modPackContents;
    public RawImage modPackPreview;
    public InputField modPackName;
    public InputField modPackTitle;
    public InputField modPackPreviewFilename;
    public InputField modPackContentFolder;
    public InputField modPackChangeNotes;
    public InputField modPackDescription;
    public InputField modPackTags;
    public Dropdown modPackVisibility;

    private const string defaultFilename = "MyNewMod.workshop.json";
    private const string defaultFolderName = "MyNewMod";
    private const string relativeBasePath = "/../WorkshopContent/";
    private string basePath;

    private WorkshopModPack currentPack;
    private string currentPackFilename;
    private UGCUpdateHandle_t currentHandle = UGCUpdateHandle_t.Invalid;

    protected CallResult<CreateItemResult_t> m_itemCreated;
    protected CallResult<SubmitItemUpdateResult_t> m_itemSubmitted;

    void Awake()
    {
        SetupDirectories();
    }

    void Start()
    {
        versionText.text = string.Format("Steam Workshop Uploader - Build {0} --- App ID: {1}", version, SteamManager.m_steamAppId);

        if(SteamManager.m_steamAppId == 0)
        {
            string error = "ERROR: Steam App ID isn't set! Make sure 'steam_appid.txt' is placed next to the executable file, and contains a single line with the app id.";
        }

        RefreshPackList();
        RefreshCurrentModPack();
    }

    void OnApplicationQuit()
    {
        if (currentPack != null)
        {
            OnCurrentModPackChanges();
            SaveCurrentModPack();
        }
        SteamAPI.Shutdown();
    }

    public void OnApplicationFocus()
    {
        RefreshPackList();

        if(currentPack != null)
        {
            RefreshCurrentModPack();
        }
    }

    public void Shutdown()
    {
        SteamAPI.Shutdown();
    }

	private void OnEnable()
    {
        basePath = Application.dataPath + relativeBasePath;
        Debug.Log("basePath is: " + basePath);

        if(SteamManager.Initialized)
        {
            m_NumberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);

            m_itemCreated = CallResult<CreateItemResult_t>.Create(OnItemCreated);
            m_itemSubmitted = CallResult<SubmitItemUpdateResult_t>.Create(OnItemSubmitted);
        }
	}

    void SetupDirectories()
    {
        basePath = Application.dataPath + relativeBasePath;

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
    }

    public string[] GetPackFilenames()
    {
        return Directory.GetFiles(basePath, "*.workshop.json", SearchOption.TopDirectoryOnly);
    }

    public void ClearPackList()
    {
        foreach(Transform child in packListRoot)
        {
            Destroy(child.gameObject);
        }
    }

    public void RefreshPackList()
    {
        ClearPackList();

        var paths = GetPackFilenames();

        // create list of buttons using prefabs
        // hook up their click events to the right function
        for(int i=0; i < paths.Length; i++)
        {
            string packPath = paths[i];
            string packName = Path.GetFileName(packPath);

            var buttonObj = Instantiate(packListButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            var button = buttonObj.GetComponent<Button>();
            button.transform.SetParent(packListRoot);

            button.GetComponentInChildren<Text>().text = packName.Replace(".workshop.json","");
            
            if (button != null)
            {
                // sneaky weirdness required here!
                // see: http://answers.unity3d.com/questions/791573/46-ui-how-to-apply-onclick-handler-for-button-gene.html
                //int buttonNumber = i;
                string fileName = packPath;
                //var e = new MenuButtonEvent(gameObject.name, buttonNumber);
                button.onClick.AddListener(() => { SelectModPack(fileName); });
            }
        }
    }

    public void RefreshCurrentModPack()
    {
        if(currentPack == null)
        {
            currentItemPanel.gameObject.SetActive(false);
            return;
        }

        currentItemPanel.gameObject.SetActive(true);

        var filename = currentPack.filename;

        submitButtonText.text = "Submit " + Path.GetFileNameWithoutExtension(filename.Replace(".workshop", ""));
        modPackContents.text = JSON.Dump(currentPack, true);

        RefreshPreview();

        modPackTitle.text = currentPack.title;
        modPackPreviewFilename.text = currentPack.previewfile;
        modPackContentFolder.text = currentPack.contentfolder;
        modPackDescription.text = currentPack.description;
        modPackTags.text = string.Join(",", currentPack.tags.ToArray());
        modPackVisibility.value = currentPack.visibility;
    }
    
    public void SelectModPack(string filename)
    {
        if(currentPack != null)
        {
            OnCurrentModPackChanges();
            SaveCurrentModPack();
        }

        var pack = WorkshopModPack.Load(filename);

        if (pack != null)
        {
            currentPack = pack;
            currentPackFilename = filename;

            RefreshCurrentModPack();
            //EditModPack(filename);
        }
    }

    public void EditModPack(string packPath)
    {
        System.Diagnostics.Process.Start(packPath);
    }

    public void RefreshPreview()
    {
        string path = basePath + currentPack.previewfile;
        
        // NOTE : intentionally set texture to null if no texture is found
        var preview = Utils.LoadTextureFromFile(path);
        modPackPreview.texture = preview;
    }

    public bool ValidateModPack(WorkshopModPack pack)
    {
        statusText.text = "Validating mod pack...";

        string path = basePath + pack.previewfile;

        var info = new FileInfo(path);
        if (info.Length >= 1024 * 1024)
        {
            statusText.text = "ERROR: Preview file must be <1MB!";
            return false;
        }

        return true;
    }

    public void OnCurrentModPackChanges()
    {
        OnChanges(currentPack);
        RefreshCurrentModPack();
    }

    public void OnChanges(WorkshopModPack pack)
    {
        // interface stuff
        pack.previewfile = modPackPreviewFilename.text;
        pack.title = modPackTitle.text;
        pack.description = modPackDescription.text;
        pack.tags = new List<string>(modPackTags.text.Split(','));
        pack.visibility = modPackVisibility.value;
    }

    public void AddModPack()
    {
        var packName = modPackName.text;

        // validate modpack name
        if (string.IsNullOrEmpty(packName) || packName.Contains("."))
        {
            statusText.text = "Bad modpack name: " + modPackName.text;
        }
        else
        {
            string filename = basePath + packName + ".workshop.json";

            var pack = new WorkshopModPack();
            pack.Save(filename);

            pack.contentfolder = modPackName.text;
            Directory.CreateDirectory(basePath + modPackName.text);
            
            RefreshPackList();

            SelectModPack(filename);

            CreateWorkshopItem();
        }
    }
    
    public void SaveCurrentModPack()
    {
        if (currentPack != null && !string.IsNullOrEmpty(currentPackFilename))
        {
            currentPack.Save(currentPackFilename);
        }
    }

    public void SubmitCurrentModPack()
    {
        if (currentPack != null)
        {
            OnChanges(currentPack);
            SaveCurrentModPack();
            
            if (ValidateModPack(currentPack))
            {
                if(string.IsNullOrEmpty(currentPack.publishedfileid))
                {
                    CreateWorkshopItem();
                }
                else
                {
                    UploadModPack(currentPack);
                }
            }
        }
    }

    private void CreateWorkshopItem()
    {
        if (string.IsNullOrEmpty(currentPack.publishedfileid))
        {
            SteamAPICall_t call = SteamUGC.CreateItem(new AppId_t(SteamManager.m_steamAppId), Steamworks.EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            m_itemCreated.Set(call, OnItemCreated);

            statusText.text = "Creating new item...";
        }
    }

    private void UploadModPack(WorkshopModPack pack)
    {
        if (string.IsNullOrEmpty(currentPack.publishedfileid))
        {
            statusText.text = "ERROR: publishedfileid is empty, try creating the workshop item first...";
            return;
        }

        ulong ulongId = ulong.Parse(pack.publishedfileid);
        var id = new PublishedFileId_t(ulongId);

        UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(new AppId_t(SteamManager.m_steamAppId), id);
        //m_itemUpdated.Set(call);
        //OnItemUpdated(call, false);

        // Only set the changenotes when clicking submit
        pack.changenote = modPackChangeNotes.text;

        currentHandle = handle;
        SetupModPack(handle, pack);
        SubmitModPack(handle, pack);
    }

    private void SetupModPack(UGCUpdateHandle_t handle, WorkshopModPack pack)
    {
        SteamUGC.SetItemTitle(handle, pack.title);
        SteamUGC.SetItemDescription(handle, pack.description);
        SteamUGC.SetItemVisibility(handle, (ERemoteStoragePublishedFileVisibility)pack.visibility);
        SteamUGC.SetItemContent(handle, basePath + pack.contentfolder);
        SteamUGC.SetItemPreview(handle, basePath + pack.previewfile);
        SteamUGC.SetItemMetadata(handle, pack.metadata);

        pack.ValidateTags();
        SteamUGC.SetItemTags(handle, pack.tags);
    }

    private void SubmitModPack(UGCUpdateHandle_t handle, WorkshopModPack pack)
    {
        SteamAPICall_t call = SteamUGC.SubmitItemUpdate(handle, pack.changenote);
        m_itemSubmitted.Set(call, OnItemSubmitted);
        //In the same way as Creating a Workshop Item, confirm the user has accepted the legal agreement. This is necessary in case where the user didn't initially create the item but is editing an existing item.
    }

    private void OnItemCreated(CreateItemResult_t callback, bool ioFailure)
    {
        if (ioFailure)
        {
			statusText.text = "Error: I/O Failure! :(";
            return;
		}

        switch(callback.m_eResult)
        {
            case EResult.k_EResultInsufficientPrivilege:
                // you're banned!
                statusText.text = "Error: Unfortunately, you're banned by the community from uploading to the workshop! Bummer. :(";
                break;
            case EResult.k_EResultTimeout:
                statusText.text = "Error: Timeout :S";
                break;
            case EResult.k_EResultNotLoggedOn:
                statusText.text = "Error: You're not logged into Steam!";
                break;
            case EResult.k_EResultBanned:
                statusText.text = "You don't have permission to upload content to this hub because they have an active VAC or Game ban.";
                break;
            case EResult.k_EResultServiceUnavailable:
                statusText.text = "The workshop server hosting the content is having issues - please retry.";
                break;
            case EResult.k_EResultInvalidParam:
                statusText.text = "One of the submission fields contains something not being accepted by that field.";
                break;
            case EResult.k_EResultAccessDenied:
                statusText.text = "There was a problem trying to save the title and description. Access was denied.";
                break;
            case EResult.k_EResultLimitExceeded:
                statusText.text = "You have exceeded your Steam Cloud quota. Remove some items and try again.";
                break;
            case EResult.k_EResultFileNotFound:
                statusText.text = "The uploaded file could not be found.";
                break;
            case EResult.k_EResultDuplicateRequest:
                statusText.text = "The file was already successfully uploaded. Please refresh.";
                break;
            case EResult.k_EResultDuplicateName:
                statusText.text = "You already have a Steam Workshop item with that name.";
                break;
            case EResult.k_EResultServiceReadOnly:
                statusText.text = "Due to a recent password or email change, you are not allowed to upload new content. Usually this restriction will expire in 5 days, but can last up to 30 days if the account has been inactive recently. ";
                break;
        }

        if(callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            /*
             * Include text next to the button that submits an item to the workshop, something to the effect of: “By submitting this item, you agree to the workshop terms of service” (including the link)
After a user submits an item, open a browser window to the Steam Workshop page for that item by calling:
SteamFriends()->ActivateGameOverlayToWebPage( const char *pchURL );
pchURL should be set to steam://url/CommunityFilePage/PublishedFileId_t replacing PublishedFileId_t with the workshop item Id.
This has the benefit of directing the author to the workshop page so that they can see the item and configure it further if necessary and will make it easy for the user to read and accept the Steam Workshop Legal Agreement.
             * */
        }

		if(callback.m_eResult == EResult.k_EResultOK)
        {
            statusText.text = "Item creation successful! Published Item ID: " + callback.m_nPublishedFileId.ToString();
			Debug.Log("Item created: Id: " + callback.m_nPublishedFileId.ToString());

            currentPack.publishedfileid = callback.m_nPublishedFileId.ToString();
            /*
            string filename = basePath + modPackName.text + ".workshop.json";

            var pack = new WorkshopModPack();
            pack.publishedfileid = callback.m_nPublishedFileId.ToString();
            pack.Save(filename);

            Directory.CreateDirectory(basePath + modPackName.text);
            
            RefreshPackList();
            */
        }
    }

    private void OnItemSubmitted(SubmitItemUpdateResult_t callback, bool ioFailure)
    {
        if (ioFailure)
        {
			statusText.text = "Error: I/O Failure! :(";
            return;
		}

        if(callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            statusText.text = "You need to accept the Steam Workshop legal agreement for this game before you can upload items!";
            return;
        }

        currentHandle = UGCUpdateHandle_t.Invalid;
        
        switch(callback.m_eResult)
        {
            case EResult.k_EResultOK:
                statusText.text = "SUCCESS! Item submitted! :D :D :D";
                break;
            case EResult.k_EResultFail:
                statusText.text = "Failed, dunno why :(";
                break;
            case EResult.k_EResultInvalidParam:
                statusText.text ="Either the provided app ID is invalid or doesn't match the consumer app ID of the item or, you have not enabled ISteamUGC for the provided app ID on the Steam Workshop Configuration App Admin page. The preview file is smaller than 16 bytes.";
                break;
            case EResult.k_EResultAccessDenied:
                statusText.text ="ERROR: The user doesn't own a license for the provided app ID.";
                break;
            case EResult.k_EResultFileNotFound:
                statusText.text = "Failed to get the workshop info for the item or failed to read the preview file.";
                break;
            case EResult.k_EResultLockingFailed:
                statusText.text = "Failed to aquire UGC Lock.";
                break;
            case EResult.k_EResultLimitExceeded:
                statusText.text = "The preview image is too large, it must be less than 1 Megabyte; or there is not enough space available on the users Steam Cloud.";
                break;
        }
    }

    private void UpdateProgressBar(UGCUpdateHandle_t handle)
    {
        ulong bytesDone;
        ulong bytesTotal;
        EItemUpdateStatus status = SteamUGC.GetItemUpdateProgress(handle, out bytesDone, out bytesTotal);

        float progress = (float)bytesDone / (float)bytesTotal;
        progressBar.value = progress;

        switch(status)
        {
            case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
                statusText.text = "Committing changes...";
                break;
            case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
                statusText.text = "Uploading preview image...";
                break;
            case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
                statusText.text = "Uploading content...";
                break;
            case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
                statusText.text = "Preparing configuration...";
                break;
            case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
                statusText.text = "Preparing content...";
                break;
            // from the docs: "The item update handle was invalid, the job might be finished, a SubmitItemUpdateResult_t call result should have been returned for it."
            // case EItemUpdateStatus.k_EItemUpdateStatusInvalid:
            //     statusText.text = "Item invalid ... dunno why! :(";
            //     break;
        }

    }


    private CallResult<NumberOfCurrentPlayers_t> m_NumberOfCurrentPlayers;

    private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bSuccess != 1 || bIOFailure)
        {
            Debug.Log("There was an error retrieving the NumberOfCurrentPlayers.");
        }
        else
        {
            Debug.Log("The number of players playing your game: " + pCallback.m_cPlayers);
        }
    }

	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
            m_NumberOfCurrentPlayers.Set(handle);
            Debug.Log("Called GetNumberOfCurrentPlayers()");
        }

	    if(currentHandle != UGCUpdateHandle_t.Invalid)
        {
            UpdateProgressBar(currentHandle);
        }
        else
        {
            progressBar.value = 0f;
        }
	}
}

[System.Serializable]
public class WorkshopModPack
{
    // gets populated when the modpack is loaded; shouldn't be serialized since it would go out of sync
    [TinyJSON.Skip]
    public string filename;

    // populated by the app, should generally be different each time anyways
    [TinyJSON.Skip]
    public string changenote = "Version 1.0";
    
    // string, because this is a ulong and JSON doesn't like em
    public string publishedfileid = "";
    public string contentfolder = "";
	public string previewfile = "";
	public int visibility = 2;          // hidden by default!
    public string title = "My New Mod Pack";
	public string description = "Description goes here";
    public string metadata = "";
    public List<string> tags = new List<string>();

    public void ValidateTags()
    {
        var config = Config.Load();

        if(!config.validateTags)
        {
            return;
        }

        for (int i = 0; i < tags.Count; i++)
        {
            // get rid of tags that aren't valid
            if (!config.validTags.Contains(tags[i]))
            {
                Debug.LogError("Removing invalid tag: " + tags[i]);
                tags.RemoveAt(i);
                i--;
            }
        }
    }

    public static WorkshopModPack Load(string filename)
    {
        WorkshopModPack pack = null;
        string jsonString = Utils.LoadTextFile(filename);
        JSON.MakeInto<WorkshopModPack>(JSON.Load(jsonString), out pack);

        pack.filename = filename;

        return pack;
    }

    public void Save(string filename)
    {
        string jsonString = JSON.Dump(this, true);
        Utils.SaveJsonToFile(filename, jsonString);

        Debug.Log("Saved modpack to file: " + filename);
    }
}
