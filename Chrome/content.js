var pageUp = 33;
var pageDown = 34;
var leftArrow = 37;
var rightArrow = 39;
var one = 49;
var two = 50;
var ctrl = 17;

document.addEventListener('keydown', function(event) {
// Call the getCurrentVideo() function to get the currently playing video element
var currentVideo = getCurrentVideo();
if (!currentVideo) return;

if (event.keyCode == pageUp) {
currentVideo.currentTime += currentVideo.duration * 0.2; 
} else if (event.keyCode == pageDown) {
currentVideo.currentTime -= currentVideo.duration * 0.2; 
} else if (event.keyCode == leftArrow && event.ctrlKey) {
currentVideo.currentTime -= (event.shiftKey ? 90 : 30); 
} else if (event.keyCode == rightArrow && event.ctrlKey) {
currentVideo.currentTime += (event.shiftKey ? 90 : 30);
} else if (event.keyCode == one && event.altKey) {
var element = getButtonElementByText('Pause');
element.focus();
} else if (event.keyCode == two && event.altKey) {
var element = getButtonElementByText('Skip Ads');
element.click();

}
});

function getButtonElementByText(text) {
  var elements = document.getElementsByTagName('button');
  for (var i = 0; i < elements.length; i++) {
    if (elements[i].textContent.includes(text)
        || elements[i].getAttribute('aria-label') && elements[i].getAttribute('aria-label').toLowerCase().indexOf(text.toLowerCase()) !== -1) {
      return elements[i];
    }
  }
  return null;
}

function getCurrentVideo() {
  // Check for the currently playing video on the main page
  var currentVideo = document.querySelector('video:focus') || document.querySelector('video[autoplay]') || document.querySelector('video:last-of-type');

  // If no currently playing video is found, return null
  return currentVideo || null;
}
