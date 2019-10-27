
var g_nodesDict={};
var baseUrl = window.location.href.replace(/\/[^/]*\.html/,"");
var imgSrc = {
  "expand":baseUrl+"/expand_button.png",
  "colapse":baseUrl+"/collapse_button.png",
  "noop":baseUrl+"/noop_button.png",
  "checked":baseUrl+"/sts_checked.png",
  "uncheck":baseUrl+"/sts_uncheck.png",
  "gray":baseUrl+"/sts_gray.png",  
};

// render item by item
function render1(lectLst) {
  document.getElementById(rootNode.id + "_childs").innerHTML = "";
  rootNode.visible = true;

  for (var i in lectLst) {
    addLectItem(rootNode,lectLst[i]);
  }
}

// render hold tree
function render2(lectLst) {
  for (var i in lectLst) {
    addNode(rootNode,lectLst[i]);
  }
  treeRender(rootNode);
}

// get data from web and render async
function render3(url) {
  document.getElementById(rootNode.id + "_childs").innerHTML = "";
  rootNode.visible = true;

  //getItems();
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      var jsTxt = xhttp.responseText;
      var items = eval("(" + jsTxt + ")");
      for (var i in items) {
        var lectItem = items[i];
        addLectItem(rootNode,lectItem);
      }
    }
  };
  xhttp.open('GET',url,true);
  xhttp.send();
}


function getLecturesSync(url) {
  var xhttp = new XMLHttpRequest();
  xhttp.open('GET',url,false);
  xhttp.send();
  if (xhttp.status == 200) {
    return eval("(" + xhttp.responseText + ")");
  } else {
    return [];
  }
}

////////////////////////////////////////////
//function getItems() {
//  var uri = "https://localhost:44336/api/Lectures";
//  fetch(uri)
//    .then(response => response.json())
//    .then(data => _displayItems(data))
//    .catch(error => console.error('Unable to get items.', error));
//}
//function _displayItems(data) {
//  data.forEach(item => {
//    addLectItem(rootNode,item);
//  });
//}
////////////////////////////////////////////
//add lecture item to root node
function addNode(root,lectItem) {
  var path = [lectItem.author, lectItem.target, lectItem.topic];
  var parent = root;
  var child;
  for (var i in path) {
    var name = path[i];
    if (parent.childs[name] != null) {
      child = parent.childs[name];
    } else {
      var id = parent.id + '_' + Object.keys(parent.childs).length;
      child = {
          "id":id,
          "name":name,
          "childs":[],
        };
      parent.childs[name] = child;
    }
    parent = child;
  }
  parent.childs[lectItem.title] = {
      "id":parent.id + '_' + lectItem.title,
      "name":lectItem.title,
      "lect":lectItem,
    };
}

//add and render lecture item
function addLectItem(root, lectItem) {
  var path = [lectItem.author, lectItem.target, lectItem.topic, lectItem.title];
  var parent = root;
  var child;
  var renderLst=[];
  for (var i in path) {
    var name = path[i];
    if (parent.childs[name] != null) {
      child = parent.childs[name];
    } else {
      var id = parent.id + '_' + Object.keys(parent.childs).length;
      child = {
          "id":id,
          "name":name,
          "childs":[],
        };
      parent.childs[name] = child;
      
      //add to render lst
      renderLst.push([parent, child]);
    }
    parent = child;
  }
  
  child.childs = null;
  child.lect=lectItem;
  
  //render nodes
  for (var i in renderLst) {
    parent = renderLst[i][0];
    child = renderLst[i][1];
    treeAdd(parent, child);
    g_nodesDict[child.id] = child;
  }
}

function treeRender(rootNode) {
  document.getElementById(rootNode.id + "_childs").innerHTML = "";
  rootNode.visible = true;
  treeAddR(rootNode);
}
function treeAddR(parentNode){
  var i;
  for (i in parentNode.childs) {
    var childNode = parentNode.childs[i];
    treeAdd(parentNode, childNode);
    g_nodesDict[childNode.id] = childNode;
    treeAddR(childNode);
  }
}
function treeAdd(parentNode,childNode) {
  //init node
  childNode.parent = parentNode;
  childNode.sts = 0;  //uncheck
  childNode.visible = true;

  var imgTxt = '<img id="[nodeId]_img" class="toggleImg" src="[imgSrc]" onclick="onImgClick('+"'[nodeId]'"+')"></img>';
  imgTxt = imgTxt.replace(/\[nodeId\]/g,childNode.id);
  imgTxt = imgTxt.replace(/\[imgSrc\]/g,imgSrc.colapse);
  if (childNode.childs == null) {
    imgTxt = '<img src="' + imgSrc.noop + '" ></img>'
  }
  
  var stsTxt = '<img id="[nodeId]_sts" class="stsImg" src="[stsSrc]" onclick="onStsClick('+"'[nodeId]'"+')"></img>';
  stsTxt = stsTxt.replace(/\[nodeId\]/g,childNode.id);
  stsTxt = stsTxt.replace(/\[stsSrc\]/g,imgSrc.uncheck);
  
  var spanTxt = '<span id="[nodeId]_span" class="txtbtn" onclick="onSpanClick('+"'[nodeId]'"+')">[nodeName]</span><br>';
  spanTxt = spanTxt.replace(/\[nodeId\]/g,childNode.id);
  spanTxt = spanTxt.replace(/\[nodeName\]/g, childNode.name);
  
  var divTxt = '<div id="[nodeId]_childs" class="childsDiv">';
  divTxt = divTxt.replace(/\[nodeId\]/g,childNode.id);
  
  var tDiv = document.getElementById(parentNode.id + "_childs");
  tDiv.innerHTML += imgTxt  + stsTxt + spanTxt + divTxt;
}
function updateChildSts(tNode) {
  var childs = tNode.childs;
  var sts = tNode.sts;
  var i;
  for (i in childs) {
    var childNode = childs[i];
    childNode.sts = sts;
    updateStsImg(childNode);
    
    updateChildSts(childNode);
  }
}
function updateParentSts(tNode, sts){
  if (tNode.id == "root") return;
  var childs = tNode.childs;
  if (sts != 2) {
    for (var i in childs) {
      var childNode = childs[i];
      if (childNode.sts != sts) { 
        sts = 2;
        break;
      }
    }
  }
  tNode.sts = sts;
  updateStsImg(tNode);
  
  //recursive
  updateParentSts(tNode.parent, sts);
}

function updateStsImg(tNode){
  var src;
  switch (tNode.sts) {
    case 0:
      src = imgSrc.uncheck;
      break;
    case 1:
      src = imgSrc.checked;
      break;
    default:
      src = imgSrc.gray;
  }
  var tImg = document.getElementById(tNode.id + "_sts");
  tImg.src = src;
}

//"2018-04-10T04:00:00.000Z"
function getDate(zTxt) {
  return zTxt.match(/.*(?=T)/);
}