using System.Collections.Generic;
using Economy.ItemsDashboard.Models;
using Economy.ItemsDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Economy.ItemsDashboard.Controllers {
    public class ItemController : Controller {
        private readonly ItemService _itemService;
        
        public ItemController(ItemService svc) {
            _itemService = svc;
        }
        
        [AllowAnonymous]
        public ActionResult<IList<Item>> Index() => View(_itemService.Get());

        [HttpGet]
        public ActionResult Create() => View();

        [HttpPost]
        public ActionResult<Item> Create([FromForm] Item item) {
            if (ModelState.IsValid) {
                _itemService.Create(item);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult<Item> Edit(string id) => View(_itemService.Get(id));

        [HttpPost]
        public ActionResult Edit(Item item) {
            if (ModelState.IsValid) {
                _itemService.Update(item);
                return RedirectToAction("Index");
            }

            return View("Submission");
        }

        [HttpGet]
        public ActionResult Delete(string id) {
            _itemService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}