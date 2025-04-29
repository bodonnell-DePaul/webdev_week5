interface Book {
  id?: number;
  title: string;
  author: string;
  year: number;
  genre: string;
  isAvailable: boolean;
  publisherId: number;
  publisher: string;
  audioBookAvailable: boolean;
}

export default Book;