using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Bai1.Models;
using PagedList;

namespace Bai1.Controllers
{
    public class BookController : Controller
    {
        private BookStoreEntity db = new BookStoreEntity();

        // GET: Book
        //public ActionResult Index()
        //{
        //    var books = db.Books.Include(b => b.Author).Include(b => b.Category);
        //    return View(books.ToList());
        //}

        public ActionResult Index(int? page)
        {
            // Khi tải trang lên nếu dữ liệu cần được nạp sẵn thì ta cần thêm ở hành động POST nếu không sẽ gặp lõi tại hành động GET. Example :Xóa dòng bên dưới để test thử ;)))))
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");

            if (page == null) page = 1;
            var books = db.Books.Include(b => b.Author).Include(b => b.Category).OrderBy(b => b.BookID);
            int pageSize = 3;
            int pageNumber = (page ?? 1);


            return View(books.ToPagedList(pageNumber, pageSize));
        }


        // action Tìm kiếm 
        [HttpGet]
        public ActionResult SearchBook(string searchString,int categoryID, int? page)
        {
            //1. Lưu tên sách muốn kiếm
            ViewBag.Keyword = searchString;

            //2.Tạo câu truy vấn liên kết 3 bảng (Book , Author , Category)
            var books = db.Books.Include(b => b.Author).Include(b => b.Category);

            //3.Tìm kiếm theo string
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                books = books.Where(b => b.Title.ToLower().Contains(searchString));
            }

            //4. Tìm Kiếm theo category (thể loại)
            if (categoryID != 0)
                books = books.Where(a => a.CategoryID == categoryID);

            //5. Tạo danh sách danh mục để hiển thị ở giao diện thông qua DropDownList
            ViewBag.CategoryID = new SelectList(db.Categories,"CategoryID", "CategoryName");

            //6. Phân trang (Nếu 0 phân trang ta ko cần phần này)
            if (page == null) page = 1;
            int pageSize = 3;
            int pageNumber = (page ?? 1);


            // Trả về trang cũ với kết quả đã có 
            return View("Index", books.OrderBy(b => b.BookID).ToPagedList(pageNumber, pageSize));
        }

        
        // GET: Book/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Book/Create
        public ActionResult Create()
        {
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "AuthorName");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Book/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "BookID,Title,AuthorID,Price,Images,CategoryID,Description,Published,ViewCount")] Book book , HttpPostedFileBase Images)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Images.ContentLength > 0)
                    {
                        string _FileName = Path.GetFileName(Images.FileName);
                        string _path = Path.Combine(Server.MapPath("~/bookimages"), _FileName);
                        Images.SaveAs(_path);
                        book.Images = _FileName;
                    }
                    db.Books.Add(book);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch { ViewBag.Message = "Không Thành Công"; }
            }

            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "AuthorName", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }

        // GET: Book/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "AuthorName", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }

        // POST: Book/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Được dùng để gửi nội dụng đến server và ở đây được để False để tránh tấn công XSS (Cross Site scripting) [tập lệnh chéo trang]
        public ActionResult Edit([Bind(Include = "BookID,Title,AuthorID,Price,Images,CategoryID,Description,Published,ViewCount")] Book book , HttpPostedFileBase Images , FormCollection form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Images != null  )
                    {
                        string _FileName = Path.GetFileName(Images.FileName);
                        string _path = Path.Combine(Server.MapPath("~/bookimages"), _FileName);
                        Images.SaveAs(_path);
                        book.Images = _FileName;

                        // Lấy đường dẫn của hình ảnh cũ để xóa

                        _path = Path.Combine(Server.MapPath("~/bookimages"), form["oldimage"]);
                        if (System.IO.File.Exists(_path))
                            System.IO.File.Delete(_path);
                    }
                    else
                        book.Images = form["oldimage"];
                    db.Entry(book).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.Message = "Không Success!!!";
                }
                return RedirectToAction("Index");
                
            }
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "AuthorName", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }

        // GET: Book/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
