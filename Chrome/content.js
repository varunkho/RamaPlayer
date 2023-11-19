var pageUp = 33;
var pageDown = 34;
var leftArrow = 37;
var upArrow = 38;
var rightArrow = 39;
var one = 49;
var two = 50;
var three = 51;
var gChar = 71;
var mChar = 77;
var shift = 16;
var ctrl = 17;
var endkey = 35;

document.addEventListener('keydown', function(event) {
if (event.keyCode == mChar && event.altKey)
expandAnswer();

// Call the getCurrentVideo() function to get the currently playing video element
var currentVideo = getCurrentVideo();
if (!currentVideo) return;

if (isVideoPlaying(currentVideo)) {
if (event.keyCode == upArrow && event.altKey)
increaseAudioVolume(currentVideo);

if (event.keyCode == gChar && event.altKey)
gotoPosition(currentVideo);

if (event.keyCode == pageUp) {
if (event.altKey)
currentVideo.currentTime += 300;
else
currentVideo.currentTime += currentVideo.duration * 0.2; 
} else if (event.keyCode == pageDown) {
if (event.altKey)
currentVideo.currentTime -= 300;
else
currentVideo.currentTime -= currentVideo.duration * 0.2; 
} else if (event.keyCode == leftArrow && event.ctrlKey) {
currentVideo.currentTime -= (event.shiftKey ? 90 : 30); 
} else if (event.keyCode == rightArrow && event.ctrlKey) {
currentVideo.currentTime += (event.shiftKey ? 90 : 30);
} else if (event.keyCode == endkey && event.shiftKey && event.ctrlKey) {
currentVideo.currentTime = currentVideo.duration;
} else if (event.keyCode == endkey && event.shiftKey) {
currentVideo.currentTime = currentVideo.duration - 2;
}
}

 if (event.keyCode == one && event.altKey) {
var element = getButtonElementByText('Pause', 'startsWith') || getButtonElementByText('Play', 'startsWith');
element.focus();
} else if (event.keyCode == two && event.altKey) {
var element = getButtonElementByText('Skip Ad', 'startsWith') || getButtonElementByText('Skip');
element.click();
} else if (event.keyCode == three && event.altKey) {
var element = getButtonElementByText('like this video', 'startsWith') || getButtonElementByText(' likes', 'includes');
if (event.shiftKey)
element.click();

element.focus();

}
});

function gotoPosition(video){
var p = prompt("Type the position in minutes .");
        var number = parseInt(p);
        if (isNaN(number)) {
            console.log("Invalid input. Please enter a valid number.");
return;
        }

video.currentTime = number * 60;
}

function increaseAudioVolume(videoElement){
var p = prompt("Type the value by which you want to increase the volume. 2 being 200%, 3 being 300%.");
        var number = parseInt(p);
        if (isNaN(number)) {
            console.log("Invalid input. Please enter a valid number.");
return;
        }

var audioCtx = new AudioContext()
var source = audioCtx.createMediaElementSource(videoElement)
var gainNode = audioCtx.createGain()
gainNode.gain.value = number // double the volume
source.connect(gainNode)
gainNode.connect(audioCtx.destination)
}

function getButtonElementByText(text, compare) {
  var elements = document.getElementsByTagName('button');
  text = text.toLowerCase();
  for (var i = 0; i < elements.length; i++) {
    if (compare == 'includes') {
      if (elements[i].textContent.toLowerCase().includes(text)
          || elements[i].getAttribute('aria-label') && elements[i].getAttribute('aria-label').toLowerCase().includes(text))
        return elements[i];
    } else     if (compare == 'startsWith') {
      if (elements[i].textContent.toLowerCase().startsWith(text)
        || elements[i].getAttribute('aria-label') && elements[i].getAttribute('aria-label').toLowerCase().startsWith(text))
        return elements[i];
} else     {
      if (elements[i].textContent.toLowerCase() == text
        || elements[i].getAttribute('aria-label') && elements[i].getAttribute('aria-label').toLowerCase() == text)
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


$(document).ready(function() {
// Get the current URL of the page
var url = window.location.href;
// Check if the URL matches https://news.ycombinator.com/
if (url.startsWith("https://news.ycombinator.com/")) {
// Find all the a elements under span elements with class 'titleline'
var titlelines = $("span.titleline > a:first-child");
// Loop through each titleline element
titlelines.each(function() {
// Wrapp it inside an h2 element
$(this).wrap("<h2></h2>");
});
// Find all the anchor elements with word 'comments' in their inner text
var comments = $("span.subline a:contains('comments')");
// Loop through each comment element
comments.each(function() {
// Add role="heading" attribute to the anchor element
$(this).wrap("<h3></h3>");
});
}
});

function isVideoPlaying(video) {
  return !video.paused && !video.ended;
}

function findClosestPreviousElementWithClass(currentElement, targetClassName) {
    // Check if the current element itself has the specified class
    if (currentElement.classList.contains(targetClassName)) {
        return currentElement;
    }
    
    // Check each ancestor element
    var ancestor = currentElement.parentElement;
    while (ancestor !== null) {
        if (ancestor.classList.contains(targetClassName)) {
            return ancestor;
        }
        
        // Check descendants of the ancestor
        var descendantsWithClass = ancestor.querySelectorAll('.' + targetClassName);
        if (descendantsWithClass.length > 0) {
            return descendantsWithClass[descendantsWithClass.length - 1]; // Return the last matching descendant
        }
        
        // Move up the DOM tree
        ancestor = ancestor.parentElement;
    }
    
    // If no matching element is found, return null
    return null;
}

function expandAnswer(){
// Usage example:
var currentElement = document.activeElement;
var targetClassName = 'qt_read_more';
var closestElement = findClosestPreviousElementWithClass(currentElement, targetClassName);

if (closestElement !== null) {
closestElement.click();
    // console.log('Found the closest element with class:', targetClassName);
    // Do something with the closest element
} else {
console.log(closestElement);
    // console.log('Element with class', targetClassName, 'not found among ancestors.');
}
}
