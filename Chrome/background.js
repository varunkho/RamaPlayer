// Initialize tab ID
var tabId = null;

// Listen for tab updates
chrome.tabs.onUpdated.addListener(function(tabId, changeInfo, tab) {
  if (changeInfo.status === "complete") {
    // Inject content script
    chrome.tabs.executeScript(tabId, { file: "content.js" });
  }
});
