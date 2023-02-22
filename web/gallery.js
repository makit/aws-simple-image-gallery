API_GATEWAY_URL = 'https://3eru9j9le4.execute-api.eu-west-1.amazonaws.com/prod'

showLoading = (container) => {
  container.innerHTML = '';
  var loadingDiv = document.createElement('div');
  loadingDiv.className = 'col-sm';
  loadingDiv.innerText = 'Loading...';
  container.appendChild(loadingDiv);
}

addCard = (container, content, isImage) => {
  var cardTextParagraph = document.createElement('p');
  cardTextParagraph.className = 'card-text';

  if (isImage) {
    var image = document.createElement('img');
    image.setAttribute('src', content);
    cardTextParagraph.appendChild(image);
  } else {
    cardTextParagraph.innerText = content;
  }

  var cardBodyDiv = document.createElement('div');
  cardBodyDiv.className = 'card-body';
  cardBodyDiv.appendChild(cardTextParagraph);

  var cardShadowDiv = document.createElement('div');
  cardShadowDiv.className = 'card shadow-sm';
  cardShadowDiv.appendChild(cardBodyDiv);

  var colDiv = document.createElement('div');
  if (isImage) {
    colDiv.className = 'col';
  } else {
    colDiv.className = 'col clickable';
  }
  colDiv.appendChild(cardShadowDiv);

  container.appendChild(colDiv);

  return colDiv;
};

clickCategory = (container, categoryName) => {
  showLoading(container);

  fetch(`${API_GATEWAY_URL}/categories/${categoryName}`).then(r=>r.json()).then(data => 
  {
    container.innerHTML = '';

    for(var image of data) {
      addCard(container, image, true)
    }

    var goBack = addCard(container, 'Go Back', false)
    goBack.addEventListener('click', () => loadCategories());
  });
};

loadCategories = () => {
  var container = document.getElementById('container');
  showLoading(container);

  fetch(`${API_GATEWAY_URL}/categories`).then(r=>r.json()).then(data => 
  {
    container.innerHTML = '';

    for(var category of data) {
      var card = addCard(container, category, false)
      card.addEventListener('click', () => clickCategory(container, '2023-02-21'));
    }   
  });
}

window.onload = () => {
  loadCategories();
};