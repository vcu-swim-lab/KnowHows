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
    document.getElementById("chatbox").innerHTML += '<div class="message_item"><div class="message_gutter">11:26 AM</div><div class="message_content"><div class="message_sender"><div class="message_sender_name">KnowHows</div> <div class=\"app_label\">APP<\/div></div> Found <b>1</b> result for <b>StringTokenizer</b>:<br />1. <div class="mention">@aplinas</div> knows and made changes to <div class="link">diff-parser-test/KeyboardReader.java</div> (6dc5d)</div></div>';
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
    document.getElementById("chatbox").innerHTML += '<div class="message_item"><div class="message_gutter">11:26 AM</div><div class="message_content"><div class="message_sender"><div class="message_sender_name">KnowHows</div> <div class=\"app_label\">APP<\/div></div> Successfully tracked <div class="link">diff-parser-test</div></div></div>';
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
    document.getElementById("chatbox").innerHTML += '<div class=\"message_item\"><div class=\"message_gutter\">11:26 AM<\/div><div class=\"message_content\"><span class=\"message_sender\"><div class=\"message_sender_name\">KnowHows<\/div> <div class=\"app_label\">APP<\/div><\/span> Available commands:<br \/><div class=\"message_attachment\"><div class=\"message_attachment_border\" style=\"background-color: #7cd197\"><\/div><div class=\"message_attachment_body\"><b>\/knowhows to [query]<\/b><br \/>Performs a natural language search on a concept<\/div><\/div><div class=\"message_attachment\"><div class=\"message_attachment_border\" style=\"background-color: #7cd197\"><\/div><div class=\"message_attachment_body\"><b>\/knowhows search [query]<\/b><br \/>Performs a literal search on a code term<\/div><\/div><div class=\"message_attachment\"><div class=\"message_attachment_border\" style=\"background-color: #5397c1\"><\/div><div class=\"message_attachment_body\">\r\n<b>\/knowhows track [repository_name | *]<\/b><br \/>Tracks and indexes one or all (*) of your repositories<\/div><\/div><div class=\"message_attachment\"><div class=\"message_attachment_border\" style=\"background-color: #5397c1\"><\/div><div class=\"message_attachment_body\"><b>\/knowhows untrack [repository_name | *]<\/b><br \/>Untracks and unindexes one or all (*) of your repositories<\/div><\/div><div class=\"message_attachment\"><div class=\"message_attachment_border\" style=\"background-color: #c16883\"><\/div><div class=\"message_attachment_body\"><b>\/knowhows help<\/b><br \/>Shows this help message<\/div><\/div><\/div><\/div>';
    var element = document.getElementById("chatbox");
    element.scrollTop = element.scrollHeight;
    document.getElementById("track").innerHTML = "";
    i = 0;
    setTimeout(track, wait_on_return);
  }
};

document.getElementById("chatbox").innerHTML = "";
help();

