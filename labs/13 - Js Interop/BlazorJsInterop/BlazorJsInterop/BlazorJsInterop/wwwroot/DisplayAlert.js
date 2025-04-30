  window.displayAlert2 = (symbol, price) => {
    if (price < 20) {
        alert(`${symbol}: $${price}!`);
    return "User alerted in the browser.";
    } else {
      return "User NOT alerted.";
    }
  };
