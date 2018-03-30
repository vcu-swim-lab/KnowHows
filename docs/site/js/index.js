var i = 0;
var search_txt = "/knowhows search StringTokenizer";
var help_txt = "/knowhows help";
var track_txt = "/knowhows track diff-parser-test";
var wait_on_enter = 1000;
var wait_on_return = 1000;
var speed = 50;

function search() {
  if (i < search_txt.length) {
    document.getElementById("track").innerHTML += search_txt.charAt(i);
    i++;
    if (i == search_txt.length) {
       setTimeout(search, wait_on_enter);
    }
    else {
       setTimeout(search, speed);
    }
  }

  else {
    document.getElementById("chatbox").innerHTML += '<div class="message_item"><div class="message_gutter">11:26 AM</div><div class="message_content"><div class="message_sender">KnowHows</div> Here\'s who knows about <b>StringTokenizer</b>:<br />1. <div class="mention">@aplinas</div> in <div class="link">AlexAplin/diff-parser-test:L50</div></div></div>';
    var element = document.getElementById("chatbox");
    element.scrollTop = element.scrollHeight;
    document.getElementById("track").innerHTML = "";
    i = 0;
  }
};


function track() {
  if (i < track_txt.length) {
    document.getElementById("track").innerHTML += track_txt.charAt(i);
    i++;
    if (i == track_txt.length) {
       setTimeout(track, wait_on_enter);
    }
    else {
       setTimeout(track, speed);
    }
  }

  else {
    document.getElementById("chatbox").innerHTML += '<div class="message_item"><div class="message_gutter">11:26 AM</div><div class="message_content"><div class="message_sender">KnowHows</div> Successfully tracked <div class="link">diff-parser-test</div></div></div>';
    var element = document.getElementById("chatbox");
    element.scrollTop = element.scrollHeight;
    document.getElementById("track").innerHTML = "";
    i = 0;
    setTimeout(search, wait_on_return);
  }
};

function help() {
  if (i < help_txt.length) {
    document.getElementById("track").innerHTML += help_txt.charAt(i);
    i++;
    if (i == help_txt.length) {
       setTimeout(help, wait_on_enter);
    }
    else {
        setTimeout(help, speed);
    }
  }

  else {
    document.getElementById("chatbox").innerHTML += '<div class="message_item"><div class="message_gutter">11:26 AM</div><div class="message_content"><div class="message_sender">KnowHows</div> Available commands:<br />/knowhows to <query> -- performs a natural language search<br />/knowhows search <query> -- performs search for explicit request<br />/knowhows track <repository name> -- tracks and indexes one of your repositories<br />/knowhows untrack <repository name> -- untracks and unindexes one of your repositories<br />/knowhows help -- shows this help message</div></div>';
    var element = document.getElementById("chatbox");
    element.scrollTop = element.scrollHeight;
    document.getElementById("track").innerHTML = "";
    i = 0;
    setTimeout(track, wait_on_return);
  }
};

document.getElementById("chatbox").innerHTML = "";
help();